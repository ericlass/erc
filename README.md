# erc Programming Language
Modern, powerful programming language for modern x64 CPUs. erc is made with performance in mind as the first and most important aspect.

# Planned Features
### Language:
- Built-in vector types that utilize modern SSE/AVX CPU extensions (already implemented)
- Type inference (no need to specify type of variable) (already implemented)
- No "null". All variables always have a valid value (already implemented)
- Pointers (already implemented)
- No object orientation, though there will be structs with methods, but no inheritence and stuff like that
- Polymorphy through simple interfaces
- Common Code sharing through traits
- Included support for SoA
- Intrinsics for certain CPU instructions

### Compiler:
- Native compilation to executable
- Only for modern x64 CPUs that support AVX2
- No LLVM. The compiler creates optimized assembler code which is then passed to the awesome [Flat Assembler](https://flatassembler.net/) to create the executable
- Currently implemented in C# until the language is ready to self-compile
- Currently only for Windows, but Linux support planned in the future
- Maximum compilation speed (hoping to compile 1M lines of code in < 1s)

## Example
Here's an example that shows some features. Please note that the syntax is very likely to change on the road to version 1.0.
```rust
//declare integer variable (with type inference, signed 64bit integer is default)
let a = 5;

//declare floating point variable (with type inference, 64bit double precision is default)
let pi = 3.1415;

//declare vector variable (with type inference, vector of 4 64bit double precision floats here)
let va = vec(1.0, 2.0, 3.0, 4.0)
let vb = vec(4.0, 3.0, 2.0, 1.0)

//add vectors (will use AVX)
let result = va + vb;

//access vector components
let x = result.0;
let y = result.1;
let z = result.2;
```
