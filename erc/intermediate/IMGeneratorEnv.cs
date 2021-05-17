using System;
using System.Collections.Generic;

namespace erc
{
    public class IMGeneratorEnv
    {
        private long _labelCounter = 0;
        private long _tempLocalCounter = 0;

        public string NewLabelName()
        {
            _labelCounter += 1;
            return "label_" + _labelCounter;
        }

        public IMOperand NewTempLocal(DataType dataType)
        {
            _tempLocalCounter += 1;
            return IMOperand.Local(dataType, _tempLocalCounter.ToString());
        }

        public void ResetTempLocals()
        {
            _tempLocalCounter = 0;
        }

    }
}
