using Ellang.Compiler.Semantic.Binding;
using Ellang.Compiler.Infra;
using Ellang.Compiler.Semantic;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Ellang.Compiler;

public sealed class AFcukingCompilation(string moduleName)
{
	public string ModuleName { get; } = moduleName;
	public LogLevel LogLevel { get; } = LogLevel.Debug;

	private readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
	{
		TypeInfoResolver = new DumpEverythingPolymorphicJsonTypeInfoResolver(),
		WriteIndented = true,
	};

	public void Compile(string source)
	{
		var lexed = new Lexer.Lexer(new ConsoleLogger<Lexer.Lexer>(LogLevel)).Parse(source);
		foreach (var token in lexed)
		{
			Console.WriteLine(token);
		}


		var result = new Parser.Parser(new ConsoleLogger<Parser.Parser>(LogLevel)).Parse(lexed);
		var json = JsonSerializer.Serialize(result, _serializerOptions);

		Console.WriteLine();
		Console.WriteLine(json);
		File.WriteAllText("./ast.txt", json);

		var reconstructed = result.Reconstruct();
		Console.WriteLine();
		Console.WriteLine(reconstructed);

		var binder = new Binder(this, new ConsoleLogger<Binder>(LogLevel));

		var coreLibModule = new ModuleSymbol(Constants.CoreLibModuleName);

		foreach (var (_, coreLibTypeName) in Constants.TypeKeywords)
		{
			binder.SymbolManager.GlobalStructTable.Add(
				new StructSymbol(
					SymbolIdent.From(coreLibTypeName, Constants.CoreLibModuleName),
					[],
					[],
					[],
					null
				));
		}

		var listSymbol = new StructSymbol(
			SymbolIdent.CoreLib("List`1"),
			[new TypeParameterSymbol(new LocalSymbolIdent("T"))],
			[new StructFieldSymbol(new LocalSymbolIdent("Count"),
				new StructTypeReferenceSymbol(
					binder.SymbolManager.GlobalStructTable.Get(
						SymbolIdent.From("Int32", Constants.CoreLibModuleName))))],
			[new StructMethodSymbol(
				new NamedFunctionSymbol(
					new LocalSymbolIdent("At").AsGlobal(),
					new TypeParameterTypeReferenceSymbol(new TypeParameterSymbol(new LocalSymbolIdent("T"))),
					[new TypeParameterSymbol(new LocalSymbolIdent("T"))],
					[new FunctionParameterSymbol(
						new LocalSymbolIdent("index"), 
						new StructTypeReferenceSymbol(
							binder.SymbolManager.GlobalStructTable.Get(SymbolIdent.CoreLib("Int32"))))],
					[],
					null),
				null)],
			null);

		binder.SymbolManager.GlobalStructTable.Add(listSymbol);

		binder.Bind(result);

		_ = 1;
	}
}
