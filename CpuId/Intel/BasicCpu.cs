﻿namespace RJCP.Diagnostics.Intel
{
    using System.Xml;

    internal class BasicCpu
    {
        private const int VendorIdFunction = 0;
        private const int ExtendedFunction = unchecked((int)0x80000000);

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicCpu"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor reads all CPU information from the current CPU node.
        /// </remarks>
        public BasicCpu()
        {
            Initialize(new CpuRegisters());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicCpu"/> class.
        /// </summary>
        /// <param name="node">The XML node containing the CPUID information.</param>
        /// <remarks>
        /// This method is used to load all CPUID information from a XML file.
        /// </remarks>
        public BasicCpu(XmlNode node)
        {
            Initialize(new CpuXmlRegisters(node));
        }

        private void Initialize(ICpuRegisters register)
        {
            CpuRegisters = register;

            CpuIdRegister vendorFunction = register.GetCpuId(VendorIdFunction, 0);
            if (vendorFunction == null) return;
            FunctionCount = vendorFunction.Result[0];
            VendorId = GetVendorId(vendorFunction);

            CpuIdRegister extendedFunction = register.GetCpuId(ExtendedFunction, 0);
            if (extendedFunction == null) return;
            if ((extendedFunction.Result[0] & ExtendedFunction) != 0)
                ExtendedFunctionCount = extendedFunction.Result[0] & 0x7FFFFFFF;
        }

        private string GetVendorId(CpuIdRegister register)
        {
            char[] vendorId = new char[12];
            int ebx = register.Result[1];
            int ecx = register.Result[2];
            int edx = register.Result[3];

            vendorId[0] = (char)(ebx & 0xFF);
            vendorId[1] = (char)((ebx >> 8) & 0xFF);
            vendorId[2] = (char)((ebx >> 16) & 0xFF);
            vendorId[3] = (char)((ebx >> 24) & 0xFF);
            vendorId[4] = (char)(edx & 0xFF);
            vendorId[5] = (char)((edx >> 8) & 0xFF);
            vendorId[6] = (char)((edx >> 16) & 0xFF);
            vendorId[7] = (char)((edx >> 24) & 0xFF);
            vendorId[8] = (char)(ecx & 0xFF);
            vendorId[9] = (char)((ecx >> 8) & 0xFF);
            vendorId[10] = (char)((ecx >> 16) & 0xFF);
            vendorId[11] = (char)((ecx >> 24) & 0xFF);
            return new string(vendorId);
        }

        public ICpuRegisters CpuRegisters { get; private set; }

        public string VendorId { get; private set; }

        public int FunctionCount { get; private set; }

        public int ExtendedFunctionCount { get; private set; }
    }
}
