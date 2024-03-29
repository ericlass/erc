﻿using System;
using System.Collections.Generic;

namespace erc
{
    public class CompilerContext
    {
        public string Source { get; set; }
        public List<Token> Tokens { get; set; }
        public AstItem AST { get; set; }
        public List<IIMObject> IMObjects { get; set; }
        public SimpleLogger Logger { get; } = new SimpleLogger();
        public string NativeCode { get; set; }

        private ProgramScope _programScope = new();
        private FunctionScope _functionScope = null;
        private BlockScope _blockScope = null;

        public void ResetScope()
        {
            _functionScope = null;
            _blockScope = null;
        }

        public void EnterFunction(Function function)
        {
            if (_functionScope != null)
                throw new Exception("Already in function!");

            _functionScope = new FunctionScope(function);
        }

        public void LeaveFunction()
        {
            if (_functionScope == null)
                throw new Exception("Trying to leave non-existing function scope!");

            _functionScope = null;
            _blockScope = null;
        }

        public void EnterBlock()
        {
            EnterBlock(null);
        }

        public void EnterBlock(string endLabel)
        {
            if (_functionScope == null)
                throw new Exception("Cannot enter block if not inside function!");

            _blockScope = new BlockScope(_blockScope, endLabel);
        }

        public string GetCurrentScopeEndLabel()
        {
            Assert.True(_blockScope != null, "Trying to get end label for scope, but not inside block scope!");
            return _blockScope.GetEndLabel();
        }

        public void LeaveBlock()
        {
            if (_blockScope == null)
                throw new Exception("Trying to leave non-existing block scope!");

            _blockScope = _blockScope.Parent;
        }

        public Symbol GetSymbol(string name)
        {
            Symbol result = null;

            if (_blockScope != null)
                result = _blockScope.GetVariable(name);

            if (result == null && _functionScope != null)
                result = _functionScope.GetParameter(name);

            return result;
        }

        public Symbol RequireSymbol(string name)
        {
            var result = GetSymbol(name);
            if (result == null)
                throw new Exception("Undeclared symbol: " + name);

            return result;
        }

        public List<Symbol> GetAllVariables()
        {
            return _blockScope.GetAllVariables();
        }

        public void AddVariable(Symbol variable)
        {
            if (_blockScope == null)
                throw new Exception("Cannot add variable, not inside block!");

            _blockScope.AddVariable(variable);
        }

        public void RemoveVariable(Symbol variable)
        {
            if (_blockScope == null)
                throw new Exception("Cannot remove variable, not inside block!");

            _blockScope.RemoveVariable(variable.Name);
        }

        public Function GetFunction(string name)
        {
            return _programScope.GetFunction(name);
        }

        public Function RequireFunction(string name)
        {
            var result = GetFunction(name);
            if (result == null)
                throw new Exception("Undeclared function: " + name);

            return result;
        }

        public Function CurrentFunction
        {
            get
            {
                if (_functionScope == null)
                    throw new Exception("Not in function currently!");

                return _functionScope.Function;
            }
        }

        public bool FunctionExists(string name)
        {
            return _programScope.GetFunction(name) != null;
        }

        public void AddFunction(Function function)
        {
            _programScope.AddFunction(function);
        }

        public void RemoveFunction(string name)
        {
            _programScope.RemoveFunction(name);
        }

    }
}
