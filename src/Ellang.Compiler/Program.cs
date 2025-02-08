using Ellang.Compiler;
using Ellang.Compiler.Infra;
using Ellang.Compiler.Lexer;
using Ellang.Compiler.Parser;
using Microsoft.Extensions.Logging;
using System.Text.Json;

const LogLevel LogLevel = LogLevel.Debug;

const string Input = """
func main(argc: int, argv: &&char): int {
	
}

func take_ref(value: &int): void {
	var another_ref: &int = value;
	var deref: int = *value;
	var x: int = deref + 3;

	# random_method(x, 3);
}

struct Test {
	X: int;
}
""";


var lexed = new Lexer(new ConsoleLogger<Lexer>(LogLevel)).Parse(Input);
foreach (var token in lexed)
{
	Console.WriteLine(token);
}

#pragma warning disable CA1869 // Cache and reuse 'JsonSerializerOptions' instances
var serializerOptions = new JsonSerializerOptions
{
	TypeInfoResolver = new DumpEverythingPolymorphicJsonTypeInfoResolver(),
	WriteIndented = true,
};
#pragma warning restore CA1869 // Cache and reuse 'JsonSerializerOptions' instances

var result = new Parser(new ConsoleLogger<Parser>(LogLevel)).Parse(lexed);
var json = JsonSerializer.Serialize(result, serializerOptions);

Console.WriteLine();
Console.WriteLine(json);
File.WriteAllText("./ast.txt", json);

var reconstructed = result.Reconstruct();
Console.WriteLine();
Console.WriteLine(reconstructed);
