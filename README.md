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

Also see [example.erc](erc/example.erc) for more code examples and [example.out](erc/example.out) for example compiler output (abstract syntax tree, immediate code, x64 assembly).

```rust
//Entry point for application
fn main()
{
    //Local variable declaration with type inference (signed 64-bit integer here)
    let a = 5;
    
    //Declare new vectors of four 64-bit double precision floating point values
    //"vec" uses type inference to determine the specific vector type
    let v1 = vec(1.0, 2.0, 3.0, 4.0);
    //You can also specify the type directly. v2 will have the same type as v1.
    let v2 = vec4d(4.0, 2.0, 2.0, 1.0);
    
    //Add the two vectors component-wise. This will use CPUs AVX vector registers and instructions.
    let result = v1 + v2;
    
    //Vector component access by zero-based index
    let x = result.0;
    let y = result.1;
    let z = result.2;
}

fn pointers()
{
    //Allocate space for 1000 four-component 32-bit floating point vectors on the heap
    //Initializing all components to 0.0 (initialization is optional)
    //Type of "vertices" will be "vec4f*"
    let vertices = new vec4f(1000, vec(0.0f, 0.0f, 0.0f, 0.0f));
    
    //Dereference pointer to get first value
    let first = *vertices;
    //Or use index to get it
    first = vertices[0];
    
    //Pointer arithmetic (vec4f is 16 bytes)
    let third = *(vertices + 2 * 16)
    
    //Write first value using dereferencing
    *vertices = vec(1.0f, 1.0f, 1.0f, 1.0f);
    //Write second value using index
    vertices[1] = vec(2.0f, 2.0f, 2.0f, 2.0f);
    
    //Need to free memory!
    del vertices;
}

//External function import from .dll files
fn ext("QueryPerformanceCounter", "Kernel32.dll") query_perf_counter(u64* performanceCount);
```
