using System;
using System.Collections.Generic;

namespace erc
{
    public partial class CodeGenerator
    {
        private Dictionary<string, Func<StorageLocation, StorageLocation, string>> _movementGenerators = null;

        private void InitMovementGenerators()
        {
            _movementGenerators = new Dictionary<string, Func<StorageLocation, StorageLocation, string>>
            {
                //----- i32 -----
                ["i32_register_register"] = (src, trgt) => "mov " + trgt.Register + ", " + src.Register,
                ["i32_register_stack"] = (src, trgt) => "mov [RBP-" + trgt.Address + "], " + src.Register,
                ["i32_stack_register"] = (src, trgt) => "mov " + trgt.Register + ", [RBP-" + src.Address + "]",
                ["i32_stack_stack"] = (src, trgt) => "mov EAX, [RBP-" + src.Address + "]\n" + "mov [RBP-" + trgt.Address + "], EAX",
                ["i32_datasection_register"] = (src, trgt) => "mov " + trgt.Register + ", [" + src.DataName + "]",
                ["i32_datasection_stack"] = (src, trgt) => "mov EAX, [" + src.DataName + "]\n" + "mov [RBP-" + trgt.Address + "], EAX",

                //----- i64 -----
                ["i64_register_register"] = (src, trgt) => "mov " + trgt.Register + ", " + src.Register,
                ["i64_register_stack"] = (src, trgt) => "mov [RBP-" + trgt.Address + "], " + src.Register,
                ["i64_stack_register"] = (src, trgt) => "mov " + trgt.Register + ", [RBP-" + src.Address + "]",
                ["i64_stack_stack"] = (src, trgt) => "mov RAX, [RBP-" + src.Address + "]\n" + "mov [RBP-" + trgt.Address + "], RAX",
                ["i64_datasection_register"] = (src, trgt) => "mov " + trgt.Register + ", [" + src.DataName + "]",
                ["i64_datasection_stack"] = (src, trgt) => "mov RAX, [" + src.DataName + "]\n" + "mov [RBP-" + trgt.Address + "], RAX",

                //----- f32 -----
                ["f32_register_register"] = (src, trgt) => "movss " + trgt.Register + ", " + src.Register,
                ["f32_register_stack"] = (src, trgt) => "movss [RBP-" + trgt.Address + "], " + src.Register,
                ["f32_stack_register"] = (src, trgt) => "movss " + trgt.Register + ", [RBP-" + src.Address + "]",
                ["f32_stack_stack"] = (src, trgt) => "movss XMM0, [RBP-" + src.Address + "]\n" + "movss [RBP-" + trgt.Address + "], XMM0",
                ["f32_datasection_register"] = (src, trgt) => "movss " + trgt.Register + ", [" + src.DataName + "]",
                ["f32_datasection_stack"] = (src, trgt) => "movss XMM0, [" + src.DataName + "]\n" + "movss [RBP-" + trgt.Address + "], XMM0",

                //----- f64 -----
                ["f64_register_register"] = (src, trgt) => "movsd " + trgt.Register + ", " + src.Register,
                ["f64_register_stack"] = (src, trgt) => "movsd [RBP-" + trgt.Address + "], " + src.Register,
                ["f64_stack_register"] = (src, trgt) => "movsd " + trgt.Register + ", [RBP-" + src.Address + "]",
                ["f64_stack_stack"] = (src, trgt) => "movsd XMM0, [RBP-" + src.Address + "]\n" + "movsd [RBP-" + trgt.Address + "], XMM0",
                ["f64_datasection_register"] = (src, trgt) => "movsd " + trgt.Register + ", [" + src.DataName + "]",
                ["f64_datasection_stack"] = (src, trgt) => "movsd XMM0, [" + src.DataName + "]\n" + "movsd [RBP-" + trgt.Address + "], XMM0",

                //----- ivec4d -----
                ["ivec4d_register_register"] = (src, trgt) => "movdqa " + trgt.Register + ", " + src.Register,
                ["ivec4d_register_stack"] = (src, trgt) => "movdqa [RBP-" + trgt.Address + "], " + src.Register,
                ["ivec4d_stack_register"] = (src, trgt) => "movdqa " + trgt.Register + ", [RBP-" + src.Address + "]",
                ["ivec4d_stack_stack"] = (src, trgt) => "movdqa XMM0, [RBP-" + src.Address + "]\n" + "movdqa [RBP-" + trgt.Address + "], XMM0",
                ["ivec4d_datasection_register"] = (src, trgt) => "movdqa " + trgt.Register + ", [" + src.DataName + "]",
                ["ivec4d_datasection_stack"] = (src, trgt) => "movdqa XMM0, [" + src.DataName + "]\n" + "movdqa [RBP-" + trgt.Address + "], XMM0",

                //----- ivec8d -----
                ["ivec8d_register_register"] = (src, trgt) => "vmovdqa " + trgt.Register + ", " + src.Register,
                ["ivec8d_register_stack"] = (src, trgt) => "vmovdqa [RBP-" + trgt.Address + "], " + src.Register,
                ["ivec8d_stack_register"] = (src, trgt) => "vmovdqa " + trgt.Register + ", [RBP-" + src.Address + "]",
                ["ivec8d_stack_stack"] = (src, trgt) => "vmovdqa YMM0, [RBP-" + src.Address + "]\n" + "vmovdqa [RBP-" + trgt.Address + "], YMM0",
                ["ivec8d_datasection_register"] = (src, trgt) => "vmovdqa " + trgt.Register + ", [" + src.DataName + "]",
                ["ivec8d_datasection_stack"] = (src, trgt) => "vmovdqa YMM0, [" + src.DataName + "]\n" + "vmovdqa [RBP-" + trgt.Address + "], YMM0",

                //----- ivec2q -----
                ["ivec2q_register_register"] = (src, trgt) => "movdqa " + trgt.Register + ", " + src.Register,
                ["ivec2q_register_stack"] = (src, trgt) => "movdqa [RBP-" + trgt.Address + "], " + src.Register,
                ["ivec2q_stack_register"] = (src, trgt) => "movdqa " + trgt.Register + ", [RBP-" + src.Address + "]",
                ["ivec2q_stack_stack"] = (src, trgt) => "movdqa XMM0, [RBP-" + src.Address + "]\n" + "movdqa [RBP-" + trgt.Address + "], XMM0",
                ["ivec2q_datasection_register"] = (src, trgt) => "movdqa " + trgt.Register + ", [" + src.DataName + "]",
                ["ivec2q_datasection_stack"] = (src, trgt) => "movdqa XMM0, [" + src.DataName + "]\n" + "movdqa [RBP-" + trgt.Address + "], XMM0",

                //----- ivec4q -----
                ["ivec4q_register_register"] = (src, trgt) => "vmovdqa " + trgt.Register + ", " + src.Register,
                ["ivec4q_register_stack"] = (src, trgt) => "vmovdqa [RBP-" + trgt.Address + "], " + src.Register,
                ["ivec4q_stack_register"] = (src, trgt) => "vmovdqa " + trgt.Register + ", [RBP-" + src.Address + "]",
                ["ivec4q_stack_stack"] = (src, trgt) => "vmovdqa YMM0, [RBP-" + src.Address + "]\n" + "vmovdqa [RBP-" + trgt.Address + "], YMM0",
                ["ivec4q_datasection_register"] = (src, trgt) => "vmovdqa " + trgt.Register + ", [" + src.DataName + "]",
                ["ivec4q_datasection_stack"] = (src, trgt) => "vmovdqa YMM0, [" + src.DataName + "]\n" + "vmovdqa [RBP-" + trgt.Address + "], YMM0",

                //----- vec4f -----
                ["vec4f_register_register"] = (src, trgt) => "movaps " + trgt.Register + ", " + src.Register,
                ["vec4f_register_stack"] = (src, trgt) => "movaps [RBP-" + trgt.Address + "], " + src.Register,
                ["vec4f_stack_register"] = (src, trgt) => "movaps " + trgt.Register + ", [RBP-" + src.Address + "]",
                ["vec4f_stack_stack"] = (src, trgt) => "movaps XMM0, [RBP-" + src.Address + "]\n" + "movaps [RBP-" + trgt.Address + "], XMM0",
                ["vec4f_datasection_register"] = (src, trgt) => "movaps " + trgt.Register + ", [" + src.DataName + "]",
                ["vec4f_datasection_stack"] = (src, trgt) => "movaps XMM0, [" + src.DataName + "]\n" + "movaps [RBP-" + trgt.Address + "], XMM0",

                //----- vec8f -----
                ["vec8f_register_register"] = (src, trgt) => "vmovaps " + trgt.Register + ", " + src.Register,
                ["vec8f_register_stack"] = (src, trgt) => "vmovaps [RBP-" + trgt.Address + "], " + src.Register,
                ["vec8f_stack_register"] = (src, trgt) => "vmovaps " + trgt.Register + ", [RBP-" + src.Address + "]",
                ["vec8f_stack_stack"] = (src, trgt) => "vmovaps YMM0, [RBP-" + src.Address + "]\n" + "vmovaps [RBP-" + trgt.Address + "], YMM0",
                ["vec8f_datasection_register"] = (src, trgt) => "vmovaps " + trgt.Register + ", [" + src.DataName + "]",
                ["vec8f_datasection_stack"] = (src, trgt) => "vmovaps YMM0, [" + src.DataName + "]\n" + "vmovaps [RBP-" + trgt.Address + "], YMM0",

                //----- vec2d -----
                ["vec2d_register_register"] = (src, trgt) => "movapd " + trgt.Register + ", " + src.Register,
                ["vec2d_register_stack"] = (src, trgt) => "movapd [RBP-" + trgt.Address + "], " + src.Register,
                ["vec2d_stack_register"] = (src, trgt) => "movapd " + trgt.Register + ", [RBP-" + src.Address + "]",
                ["vec2d_stack_stack"] = (src, trgt) => "movapd XMM0, [RBP-" + src.Address + "]\n" + "movapd [RBP-" + trgt.Address + "], XMM0",
                ["vec2d_datasection_register"] = (src, trgt) => "movapd " + trgt.Register + ", [" + src.DataName + "]",
                ["vec2d_datasection_stack"] = (src, trgt) => "movapd XMM0, [" + src.DataName + "]\n" + "movapd [RBP-" + trgt.Address + "], XMM0",

                //----- vec4d -----
                ["vec4d_register_register"] = (src, trgt) => "vmovapd " + trgt.Register + ", " + src.Register,
                ["vec4d_register_stack"] = (src, trgt) => "vmovapd [RBP-" + trgt.Address + "], " + src.Register,
                ["vec4d_stack_register"] = (src, trgt) => "vmovapd " + trgt.Register + ", [RBP-" + src.Address + "]",
                ["vec4d_stack_stack"] = (src, trgt) => "vmovapd YMM0, [RBP-" + src.Address + "]\n" + "vmovapd [RBP-" + trgt.Address + "], YMM0",
                ["vec4d_datasection_register"] = (src, trgt) => "vmovapd " + trgt.Register + ", [" + src.DataName + "]",
                ["vec4d_datasection_stack"] = (src, trgt) => "vmovapd YMM0, [" + src.DataName + "]\n" + "vmovapd [RBP-" + trgt.Address + "], YMM0",
            };
        }

        private string getMovementId(DataType dataType, StorageLocation src, StorageLocation trgt)
        {
            var result = dataType.MainType + "_" + src.Kind + "_" + trgt.Kind;
            return result.ToLower();
        }

        private string Move(DataType dataType, StorageLocation source, StorageLocation target)
        {
            var id = getMovementId(dataType, source, target);
            var generator = _movementGenerators[id];
            return generator(source, target);
        }

    }
}
