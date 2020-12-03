﻿namespace RJCP.Diagnostics.Intel
{
    using System;
    using System.Text;
    using System.Xml;
#if DEBUG
    using System.Collections.Generic;
#endif

    /// <summary>
    /// The base abstract class for which all 80x86 CPU information implementations should derive.
    /// </summary>
    public abstract class GenericIntelCpuBase : ICpuIdX86
    {
        internal const int FeatureInformationFunction = 1;
        internal const int ExtendedFeatureFunction = 7;
        internal const int ExtendedProcessorState = 13;
        internal const int ExtendedInformationFunction = unchecked((int)0x80000001);
        internal const int ProcessorBrand1Function = unchecked((int)0x80000002);
        internal const int ProcessorBrand2Function = unchecked((int)0x80000003);
        internal const int ProcessorBrand3Function = unchecked((int)0x80000004);
        internal const int ExtendedLmApicId = unchecked((int)0x80000008);
        internal const int ExtendedEncMem = unchecked((int)0x8000001F);

        private readonly BasicCpu m_Cpu;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3442:\"abstract\" classes should not have \"public\" constructors", Justification = "User code shouldn't instantiate this")]
        internal GenericIntelCpuBase(BasicCpu cpu)
        {
            m_Cpu = cpu;
            Features = new CpuFeatures();
        }

        /// <inheritdoc/>
        public virtual CpuVendor CpuVendor
        {
            get { return CpuVendor.Unknown; }
        }

        /// <inheritdoc/>
        public string VendorId
        {
            get { return m_Cpu.VendorId; }
        }

        /// <inheritdoc/>
        public ICpuRegisters Registers
        {
            get { return m_Cpu.CpuRegisters; }
        }

        /// <inheritdoc/>
        public string Description { get; protected set; }

        /// <inheritdoc/>
        public string BrandString { get; protected set; }

        /// <inheritdoc/>
        public int ProcessorSignature { get; protected set; }

        /// <inheritdoc/>
        public int Family { get; protected set; }

        /// <inheritdoc/>
        public int Model { get; protected set; }

        /// <inheritdoc/>
        public int Stepping { get; protected set; }

        /// <inheritdoc/>
        public int ProcessorType { get; protected set; }

        /// <inheritdoc/>
        public CpuFeatures Features { get; private set; }

        /// <summary>
        /// Gets the brand string from registers 80000002-80000004h.
        /// </summary>
        /// <returns>
        /// The decoded brand string, or <see langword="null"/> if the CPU doesn't support this feature.
        /// </returns>
        protected string GetProcessorBrandString()
        {
            if (m_Cpu.ExtendedFunctionCount >= 4) {
                StringBuilder brand = new StringBuilder(50);
                WriteDescription(brand, m_Cpu.CpuRegisters.GetCpuId(ProcessorBrand1Function, 0));
                WriteDescription(brand, m_Cpu.CpuRegisters.GetCpuId(ProcessorBrand2Function, 0));
                WriteDescription(brand, m_Cpu.CpuRegisters.GetCpuId(ProcessorBrand3Function, 0));
                return brand.ToString().Trim();
            }
            return null;
        }

        private void WriteDescription(StringBuilder brand, CpuIdRegister register)
        {
            if (register == null) return;

            int eax = register.Result[0];
            int ebx = register.Result[1];
            int ecx = register.Result[2];
            int edx = register.Result[3];

            Append(brand, eax & 0xFF);
            Append(brand, (eax >> 8) & 0xFF);
            Append(brand, (eax >> 16) & 0xFF);
            Append(brand, (eax >> 24) & 0xFF);
            Append(brand, ebx & 0xFF);
            Append(brand, (ebx >> 8) & 0xFF);
            Append(brand, (ebx >> 16) & 0xFF);
            Append(brand, (ebx >> 24) & 0xFF);
            Append(brand, ecx & 0xFF);
            Append(brand, (ecx >> 8) & 0xFF);
            Append(brand, (ecx >> 16) & 0xFF);
            Append(brand, (ecx >> 24) & 0xFF);
            Append(brand, edx & 0xFF);
            Append(brand, (edx >> 8) & 0xFF);
            Append(brand, (edx >> 16) & 0xFF);
            Append(brand, (edx >> 24) & 0xFF);
        }

        private void Append(StringBuilder brand, int value)
        {
            if (value == 0) return;
            brand.Append((char)value);
        }

#if DEBUG
        // This section adds debug checks, to ensure that a feidl is not defined more than once. If it is, DEBUG mode
        // will cause an exception, indicating a programmatic error.

        private class BitMask
        {
            private readonly Dictionary<int, int> m_RegisterMask = new Dictionary<int, int>();

            public bool Set(int register, int mask)
            {
                if (!m_RegisterMask.TryGetValue(register, out int currentMask)) {
                    m_RegisterMask.Add(register, currentMask);
                }
                if ((currentMask & mask) != 0) return false;
                currentMask |= mask;
                m_RegisterMask[register] = currentMask;
                return true;
            }
        }

        private class SubFunction
        {
            private readonly Dictionary<int, BitMask> m_SubFunction = new Dictionary<int, BitMask>();

            public bool Set(int subFunction, int register, int mask)
            {
                if (!m_SubFunction.TryGetValue(subFunction, out BitMask bitMask)) {
                    bitMask = new BitMask();
                    m_SubFunction.Add(subFunction, bitMask);
                }
                return bitMask.Set(register, mask);
            }
        }

        private class MainFunction
        {
            private readonly Dictionary<int, SubFunction> m_Function = new Dictionary<int, SubFunction>();

            public bool Set(int function, int subFunction, int register, int mask)
            {
                if (!m_Function.TryGetValue(function, out SubFunction subFunctionMap)) {
                    subFunctionMap = new SubFunction();
                    m_Function.Add(function, subFunctionMap);
                }
                return subFunctionMap.Set(subFunction, register, mask);
            }
        }

        private readonly MainFunction m_MainFunction = new MainFunction();
#endif

        /// <summary>
        /// Tests a bit in the feature flag.
        /// </summary>
        /// <param name="feature">The name of the CPU feature, e.g. "FPU".</param>
        /// <param name="register">The feature register obtained from a query of CPUID.</param>
        /// <param name="result">The register to query, 0 is EAX, to 3 for EDX.</param>
        /// <param name="bit">The bit to test for.</param>
        protected void TestFeature(string feature, CpuIdRegister register, int result, int bit)
        {
            TestFeature(feature, register, result, bit, false);
        }

        /// <summary>
        /// Tests a bit in the feature flag.
        /// </summary>
        /// <param name="feature">The name of the CPU feature, e.g. "FPU".</param>
        /// <param name="register">The feature register obtained from a query of CPUID.</param>
        /// <param name="result">The register to query, 0 is EAX, to 3 for EDX.</param>
        /// <param name="bit">The bit to test for.</param>
        /// <param name="invert">Inverts the result if <see langword="true"/>, otherwise feature is set if the bit is one.</param>
        protected void TestFeature(string feature, CpuIdRegister register, int result, int bit, bool invert)
        {
            bool value = (register.Result[result] & (1 << bit)) != 0;
            if (invert) value = !value;
            Features[feature] = value;

#if DEBUG
            if (!m_MainFunction.Set(register.Function, register.SubFunction, result, 1 << bit))
                throw new InvalidOperationException("Bit was already set");
#endif
        }

        /// <summary>
        /// Adds a bit field that a feature is reserved, only if it is set.
        /// </summary>
        /// <param name="register">The feature register obtained from a query of CPUID.</param>
        /// <param name="result">The register to query, 0 is EAX, to 3 for EDX.</param>
        /// <param name="mask">The bit mask of reserved bits set to 1.</param>
        /// <remarks>
        /// This should be used after testing for all features, to ensure that any unknown features are tested for and
        /// set with the generic name <c>CPUID(XXh).REG[bit]</c> or <c>CPUID(EAX=XXh,ECX=XXh).REG[bit]</c>, where the
        /// REG is one of EAX, EBX, ECX, EDX and the bit is the bit given in <paramref name="mask"/> which is a bit
        /// field of reserved features. CPUs in the future may set these bits and so it's useful to see if there are new
        /// features which are not defined. Bits that are "don't care" in the specifications should not be part of the
        /// bit field, but as most bits are "reserved" are usually zero, so that future processors can set them as set
        /// when a new feature is defined.
        /// </remarks>
        protected void ReservedFeature(CpuIdRegister register, int result, int mask)
        {
#if DEBUG
            CheckReservedBitMask(register, result, mask);
#endif

            int bit = 0;
            uint checkMask = unchecked((uint)(mask & register.Result[result]));
            while (checkMask != 0) {
                if ((checkMask & 0x01) != 0) {
                    Features[GetReservedFeatureName(register, result, bit)] = true;
                }
                checkMask >>= 1;     // Unsigned means zero is rolled into MSB, so we don't need to clear the MSB.
                bit++;
            }
        }

#if DEBUG
        private void CheckReservedBitMask(CpuIdRegister register, int result, int mask)
        {
            int bit = 0;
            uint checkMask = unchecked((uint)mask);
            while (checkMask != 0) {
                if ((checkMask & 0x01) != 0 &&
                    !m_MainFunction.Set(register.Function, register.SubFunction, result, 1 << bit)) {
                    string message = string.Format("CPUID[EAX={0:X2}h,ECX={1:X2}h].{2}[{3}] already set",
                        register.Function, register.SubFunction, GetRegisterName(result), bit);
                    throw new InvalidOperationException(message);
                }
                checkMask >>= 1;
                bit++;
            }
        }
#endif

        private string GetReservedFeatureName(CpuIdRegister register, int result, int bit)
        {
            if (register.SubFunction == 0) {
                return string.Format("CPUID({0:X2}h).{1}[{2}]",
                    register.Function, GetRegisterName(result), bit);
            }
            return string.Format("CPUID(EAX={0:X2}h,ECX={1:X2}h).{2}[{3}]",
                register.Function, register.SubFunction, GetRegisterName(result), bit);
        }

        private string GetRegisterName(int result)
        {
            switch (result) {
            case 0: return "EAX";
            case 1: return "EBX";
            case 2: return "ECX";
            case 3: return "EDX";
            default: return string.Format("R{0:X2}", result);
            }
        }

        /// <summary>
        /// Writes the cached CPUID registers (those found in <see cref="Registers" /> to an XML file.
        /// </summary>
        /// <param name="fileName">Name of the file to write to.</param>
        /// <exception cref="ArgumentNullException"><paramref name="fileName"/> may not be <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="fileName"/> may not be an empty string.</exception>
        public void Save(string fileName)
        {
            if (fileName == null) throw new ArgumentNullException(nameof(fileName));
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentException("File name may not be empty", nameof(fileName));
            using (XmlWriter xmlWriter = XmlWriter.Create(fileName, SaveXmlSettings())) {
                xmlWriter.WriteStartElement("cpuid");
                Save(xmlWriter);
                xmlWriter.WriteEndElement();
            }
        }

        /// <summary>
        /// Writes the cached CPUID registers (those found in <see cref="Registers"/> to an XML writer.
        /// </summary>
        /// <param name="xmlWriter">The XML writer to write to.</param>
        /// <exception cref="ArgumentNullException"><paramref name="xmlWriter"/> may not be <see langword="null"/>.</exception>
        public void Save(XmlWriter xmlWriter)
        {
            if (xmlWriter == null) throw new ArgumentNullException(nameof(xmlWriter));
            xmlWriter.WriteStartElement("processor");
            xmlWriter.WriteAttributeString("type", "x86");
            WriteRegisters(xmlWriter);
            xmlWriter.WriteEndElement();
        }

        private XmlWriterSettings SaveXmlSettings()
        {
            XmlWriterSettings xmlSettings = new XmlWriterSettings {
                CloseOutput = true,
                ConformanceLevel = ConformanceLevel.Document,
                Encoding = Encoding.UTF8,
                Indent = true,
                IndentChars = "\t",
                NewLineOnAttributes = false
            };
            return xmlSettings;
        }

        private void WriteRegisters(XmlWriter xmlWriter)
        {
            foreach (CpuIdRegister register in Registers) {
                xmlWriter.WriteStartElement("register");
                xmlWriter.WriteAttributeString("eax", register.Function.ToString("X8"));
                xmlWriter.WriteAttributeString("ecx", register.SubFunction.ToString("X8"));
                string value = string.Format("{0:X8},{1:X8},{2:X8},{3:X8}",
                    register.Result[0], register.Result[1], register.Result[2], register.Result[3]);
                xmlWriter.WriteString(value);
                xmlWriter.WriteEndElement();
            }
        }
    }
}
