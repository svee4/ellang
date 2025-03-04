using Ellang.Compiler;
using Ellang.Compiler.Compilation;
using Ellang.Compiler.Infra;
using Ellang.Compiler.Lexer;
using Ellang.Compiler.Parser;
using Microsoft.Extensions.Logging;
using System.Text.Json;

const string Input = """
func main(argc: int, argv: &&char): int {
	
}

func sample(value: &int, list: Core::List<int>): void {
	var another_ref: &int = value;
	var deref: int = *value;
	var x: int = deref 3;

	var y: int = list[3];

	var eq1: bool = x == deref;
	var eq2: bool = x < deref;
	var eq3: bool = eq1 && eq2;

	var bitwised: int = x & deref;

	_ = random_method(x, 3);
}

struct Test {
	X: int;
}
""";

new AFcukingCompilation("TestCompilation").Compile(Input);
