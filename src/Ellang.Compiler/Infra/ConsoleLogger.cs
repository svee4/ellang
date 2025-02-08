using Microsoft.Extensions.Logging;

namespace Ellang.Compiler.Infra;

public sealed class ConsoleLogger<T> : ILogger<T>
{
	private readonly LogLevel _logLevel;
	private readonly string _categoryName;
	private readonly Stack<object> _scopes = [];

	public ConsoleLogger(LogLevel logLevel)
	{
		_logLevel = logLevel;
		_categoryName = typeof(T).FullName ?? throw new InvalidOperationException($"Could not get type name {typeof(T)}");
	}

	public IDisposable? BeginScope<TState>(TState state) where TState : notnull
	{
		this.LogTrace("State push: {State}", state);
		_scopes.Push(state);
		return new ScopeDisposer(_scopes.Count, _scopes);
	}

	public bool IsEnabled(LogLevel logLevel) => logLevel >= _logLevel;

	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
	{
		if (!IsEnabled(logLevel))
			return;

		var scopes = _scopes.Count > 0
			? $" - {string.Join(" -> ", _scopes.Reverse())}"
			: null;

		Console.WriteLine($"[{logLevel}] ({_categoryName}{scopes}) {formatter(state, exception)}");
	}

	private sealed class ScopeDisposer(int count, Stack<object> scopes) : IDisposable
	{
		private readonly int _count = count;
		private readonly Stack<object> _scopes = scopes;

		public void Dispose()
		{
			if (_count != _scopes.Count)
				throw new InvalidOperationException("Attempt to dispose scope at the wrong level");

			_ = _scopes.Pop();
		}
	}
}
