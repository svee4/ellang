namespace Ellang.Compiler;

public static class Constants
{
	public static ReadOnlySpan<char> ValidIdentifierInitialChars => "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_";
	public static ReadOnlySpan<char> ValidIdentifierChars => "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_123456789";
}
