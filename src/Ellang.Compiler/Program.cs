using Ellang.Compiler;

const string Input = """
func main(argc: int, argv: &&byte): int {
	
}

func sample(value: &int, list: Core::List<int>): int {
	var another_ref: &int = value;
	var deref: int = *value;
	var x: int = deref + 3;

	var y: int = list.At(3);

	var eq1: bool = x == deref;
	var eq2: bool = x < deref;
	var eq3: bool = eq1 && eq2;

	var bitwised: int = x & deref;

	_ = sample(x, 3);

	var ret: int = { 
		var temp: int = 3;
		yield temp;
	};

	return 3;
}

struct Test {
	X: int;
}
""";

new AFcukingCompilation("TestCompilation").Compile(Input);
