﻿namespace RJCP.Diagnostics.CpuId.Intel
{
    using System.IO;
    using CodeQuality.NUnitExtensions;
    using NUnit.Framework;

    /// <summary>
    /// Test Fixture for interpreting Intel CPUID results for various CPUs
    /// </summary>
    /// <remarks>
    /// The test cases use the CPuIdXmlFactory to read the registers from an XML file. All test cases here limit
    /// themselves to valid XML files for process x86, obtained from an Intel CPU.
    /// </remarks>
    [TestFixture]
    public class GenuineIntelCpuTest
    {
        private readonly static string TestResources = Path.Combine(Deploy.TestDirectory, "TestResources", "GenuineIntel");

        private static readonly string[] CpuId01Ecx = new[] {
            "SSE3", "PCLMULQDQ", "DTES64", "MONITOR", "DS-CPL", "VMX", "SMX", "EIST",
            "TM2", "SSSE3", "CNXT-ID", "SDBG", "FMA", "CMPXCHG16B", "xTPR", "PDCM",
            "", "PCID", "DCA", "SSE4.1", "SSE4.2", "x2APIC", "MOVBE", "POPCNT",
            "TSC-DEADLINE", "AESNI", "XSAVE", "OSXSAVE", "AVX", "F16C", "RDRAND", "HYPERVISOR"
        };

        private static readonly string[] CpuId01Edx = new[] {
            "FPU", "VME", "DE", "PSE", "TSC", "MSR", "PAE", "MCE",
            "CX8", "APIC", "", "SEP", "MTRR", "PGE", "MCA", "CMOV",
            "PAT", "PSE-36", "PSN", "CLFSH", "", "DS", "ACPI", "MMX",
            "FXSR", "SSE", "SSE2", "SS", "HTT", "TM", "IA64", "PBE"
        };

        private static readonly string[] CpuId07Ebx = new[] {
            "FSGSBASE", "IA32_TSC_ADJUST", "SGX", "BMI1", "HLE", "AVX2", "FDP_EXCPTN_ONLY", "SMEP",
            "BMI2", "ERMS", "INVPCID", "RTM", "RDT-M", "FPU-CS Dep", "MPX", "RDT-A",
            "AVX512F", "AVX512DQ", "RDSEED", "ADX", "SMAP", "AVX512_IFMA", "", "CLFLUSHOPT",
            "CLWB", "INTEL_PT", "AVX512PF", "AVX512ER", "AVX512CD", "SHA", "AVX512BW", "AVX512VL"
        };

        // Note, AMD Doc #24594, r3.31 says RDPID is EBX[22], Intel spec says RDPID is ECX[22].
        private static readonly string[] CpuId07Ecx = new[] {
            "PREFETCHWT1", "AVX512_VBMI", "UMIP", "PKU", "OSPKE", "WAITPKG", "AVX512_VBMI2", "CET_SS",
            "GFNI", "VAES", "VPCLMULQDQ", "AVX512_VNNI", "AVX512_BITALG", "", "AVX512_POPCNTDQ", "5L_PAGE",
            "", null, null, null, null, null, "RDPID", "",
            "", "CLDEMOTE", "", "MOVDIRI", "MOVDIR64B", "ENQCMD", "SGX_LC", "PKS"
        };

        private static readonly string[] CpuId07Edx = new[] {
            "", "", "AVX512_4NNIW", "AVX512_4FMAPS", "FSRM", "", "", "",
            "AVX512_VP2INTERSECT", "SRBDS_CTRL", "MD_CLEAR", "", "", "TSX_FORCE_ABORT", "SERIALIZE", "Hybrid",
            "TSXLDTRK", "", "PCONFIG", "LBR", "CET_IBT", "", "AMX_BF16", "",
            "AMX_TILE", "AMX_INT8", "IBRS_IBPB", "STIBP", "L1D_FLUSH", "IA32_ARCH_CAPABILITIES", "IA32_CORE_CAPABILITIES", "SSBD"
        };

        private static readonly string[] CpuId13Eax = new[] {
            "XSAVEOPT", "XSAVEC", "XGETBV", "XSAVES", "", "", "", "",
            "", "", "", "", "", "", "", "",
            "", "", "", "", "", "", "", "",
            "", "", "", "", "", "", "", ""
        };

        private static readonly string[] CpuId81Ecx = new[] {
            "AHF64", "", "", "", "", "ABM", "", "",
            "PREFETCHW", "", "", "", "", "", "", "",
            "", "", "", "", "", "", "", "",
            "", "", "", "", "", "", "", ""
        };

        private static readonly string[] CpuId81Edx = new[] {
            "", "", "", "", "", "", "", "",
            "", "", "", "SYSCALL", "", "", "", "",
            "", "", "", "", "XD", "", "", "",
            "", "", "1GB_PAGE", "RDTSCP", "", "LM", "", ""
        };

        private FeatureCheck FeatureCheck { get; set; }

        public GenuineIntelCpuTest()
        {
            FeatureCheck = new FeatureCheck();
            FeatureCheck.AddFeatureSet("standard", "CPUID[01h].ECX", CpuId01Ecx);
            FeatureCheck.AddFeatureSet("standard", "CPUID[01h].EDX", CpuId01Edx);
            FeatureCheck.AddFeatureSet("standard", "CPUID[07h].EBX", CpuId07Ebx);
            FeatureCheck.AddFeatureSet("standard", "CPUID[07h].ECX", CpuId07Ecx);
            FeatureCheck.AddFeatureSet("standard", "CPUID[07h].EDX", CpuId07Edx);
            FeatureCheck.AddFeatureSet("procstate", "CPUID[0Dh,01h].EAX", CpuId13Eax);
            FeatureCheck.AddFeatureSet("extended", "CPUID[80000001h].ECX", CpuId81Ecx);
            FeatureCheck.AddFeatureSet("extended", "CPUID[80000001h].EDX", CpuId81Edx);
        }

        private GenuineIntelCpu GetCpu(string fileName)
        {
            string fullPath = Path.Combine(TestResources, fileName);
            FeatureCheck.LoadCpu(fullPath);
            GenuineIntelCpu x86cpu = FeatureCheck.Cpu as GenuineIntelCpu;
            Assert.That(x86cpu, Is.Not.Null);
            Assert.That(x86cpu.CpuVendor, Is.EqualTo(CpuVendor.GenuineIntel));
            Assert.That(x86cpu.VendorId, Is.EqualTo("GenuineIntel"));
            return x86cpu;
        }

        private void CheckSignature(int signature)
        {
            Assert.That(FeatureCheck.Cpu.ProcessorSignature,
                Is.EqualTo(signature), "Signature incorrect");
            Assert.That(FeatureCheck.Cpu.Stepping,
                Is.EqualTo(signature & 0xF), "Stepping incorrect");
            Assert.That(FeatureCheck.Cpu.Model,
                Is.EqualTo(((signature >> 4) & 0xF) + ((signature >> 12) & 0xF0)), "Model incorrect");
            Assert.That(FeatureCheck.Cpu.Family,
                Is.EqualTo(((signature >> 8) & 0xF) + ((signature >> 20) & 0xFF)), "Family incorrect");
            Assert.That(FeatureCheck.Cpu.ProcessorType,
                Is.EqualTo((signature >> 12) & 0x3), "Processor Type incorrect");
        }

        [Test]
        public void CheckDescription()
        {
            // We just want to load any file, to get the IntelCPU to check for descriptions
            GetCpu("Pentium4.xml");
            FeatureCheck.AssertOnMissingDescription();
        }

        [Test]
        public void Pentium4()
        {
            GenuineIntelCpu cpu = GetCpu("Pentium4.xml");
            CheckSignature(0xF27);
            FeatureCheck.Check("standard", 0x00004400, 0xBFEBFBFF);
            FeatureCheck.AssertOnDifference();
            Assert.That(cpu.Description, Is.EqualTo("Intel(R) Pentium(R) 4 CPU 2.53GHz"));
        }

        [Test]
        public void PentiumM()
        {
            GenuineIntelCpu cpu = GetCpu("Dell_M70.xml");
            CheckSignature(0x6D8);
            FeatureCheck.Check("standard", 0x00000180, 0xAFE9FBFF);
            FeatureCheck.Check("extended", 0x00000000, 0x00100000);
            FeatureCheck.AssertOnDifference();
            Assert.That(cpu.Description, Is.EqualTo("Intel(R) Pentium(R) M processor 2.00GHz"));
        }

        [Test]
        public void Pentium4Mobile()
        {
            GenuineIntelCpu cpu = GetCpu("Dell_c840.xml");
            CheckSignature(0xF24);
            FeatureCheck.Check("standard", 0x00000000, 0x3FEBF9FF);
            FeatureCheck.Check("extended", 0x00000000, 0x00000000);
            FeatureCheck.AssertOnDifference();
            Assert.That(cpu.Description, Is.EqualTo("Intel(R) Pentium(R) 4 Mobile CPU 1.80GHz"));
        }

        [Test]
        public void CoreQuadQ9450()
        {
            GenuineIntelCpu cpu = GetCpu("Core2Quad.xml");
            CheckSignature(0x10677);
            FeatureCheck.Check("standard", 0x0008E3FD, 0xBFEBFBFF);
            FeatureCheck.Check("extended", 0x00000001, 0x20100000);
            FeatureCheck.AssertOnDifference();
            Assert.That(cpu.Description, Is.EqualTo("Intel(R) Core(TM)2 Quad  CPU   Q9450  @ 2.66GHz"));
        }

        [Test]
        public void CoreDuoT2700()
        {
            GenuineIntelCpu cpu = GetCpu("Dell_M65.xml");
            CheckSignature(0x6EC);
            FeatureCheck.Check("standard", 0x0000C1A9, 0xBFE9FBFF);
            FeatureCheck.Check("extended", 0x00000000, 0x00100000);
            FeatureCheck.AssertOnDifference();
            Assert.That(cpu.Description, Is.EqualTo("Intel(R) Core(TM) Duo CPU      T2700  @ 2.33GHz"));
        }

        [Test]
        public void Corei7_920()
        {
            GenuineIntelCpu cpu = GetCpu("i7-920.xml");
            CheckSignature(0x106A5);
            FeatureCheck.Check("standard", 0x0098E3BD, 0xBFEBFBFF);
            FeatureCheck.Check("extended", 0x00000001, 0x28100800);
            FeatureCheck.AssertOnDifference();
            Assert.That(cpu.Description, Is.EqualTo("Intel(R) Core(TM) i7 CPU         920  @ 2.67GHz"));
        }

        [Test]
        public void Xeon_W3540()
        {
            GenuineIntelCpu cpu = GetCpu("xeon-W3540.xml");
            CheckSignature(0x106A5);
            FeatureCheck.Check("standard", 0x009CE3BD, 0xBFEBFBFF);
            FeatureCheck.Check("extended", 0x00000001, 0x28100800);
            FeatureCheck.AssertOnDifference();
            Assert.That(cpu.Description, Is.EqualTo("Intel(R) Xeon(R) CPU           W3540  @ 2.93GHz"));
        }

        [Test]
        public void Corei3_2120T()
        {
            GenuineIntelCpu cpu = GetCpu("i3-2120T.xml");
            CheckSignature(0x206A7);
            FeatureCheck.Check("standard", 0x9C982203, 0x1FEBFBFF);
            FeatureCheck.Check("extended", 0x00000001, 0x28100800);
            FeatureCheck.AssertOnDifference();
            Assert.That(cpu.Description, Is.EqualTo("Intel(R) Core(TM) i3-2120T CPU @ 2.60GHz"));
        }

        [Test]
        public void Corei7_2630QM()
        {
            GenuineIntelCpu cpu = GetCpu("i7-2630QM.xml");
            CheckSignature(0x206A7);
            FeatureCheck.Check("standard", 0x1FBAE3BF, 0xBFEBFBFF);
            FeatureCheck.Check("extended", 0x00000001, 0x28100800);
            FeatureCheck.AssertOnDifference();
            Assert.That(cpu.Description, Is.EqualTo("Intel(R) Core(TM) i7-2630QM CPU @ 2.00GHz"));
        }

        [Test]
        public void Corei5_3317U()
        {
            GenuineIntelCpu cpu = GetCpu("i5-3317U_SurfacePro.xml");
            CheckSignature(0x306A9);
            FeatureCheck.Check("standard", 0x7FBAE3BF, 0xBFEBFBFF, 0x00000281, 0x00000000, 0x00000000);
            FeatureCheck.Check("extended", 0x00000001, 0x28100800);
            FeatureCheck.AssertOnDifference();
            Assert.That(cpu.Description, Is.EqualTo("Intel(R) Core(TM) i5-3317U CPU @ 1.70GHz"));
        }

        [Test]
        public void Corei7_3820QM()
        {
            GenuineIntelCpu cpu = GetCpu("i7-3820QM.xml");
            CheckSignature(0x306A9);
            FeatureCheck.Check("standard", 0x7FBAE3FF, 0xBFEBFBFF, 0x00000281, 0x00000000, 0x00000000);
            FeatureCheck.Check("extended", 0x00000001, 0x28100800);
            FeatureCheck.AssertOnDifference();
            Assert.That(cpu.Description, Is.EqualTo("Intel(R) Core(TM) i7-3820QM CPU @ 2.70GHz"));
        }

        [Test]
        public void Corei7_4930K()
        {
            GenuineIntelCpu cpu = GetCpu("i7-4930K.xml");
            CheckSignature(0x306E4);
            FeatureCheck.Check("standard", 0x7FBEE3BF, 0xBFEBFBFF, 0x00000281, 0x00000000, 0x00000000);
            FeatureCheck.Check("extended", 0x00000001, 0x2C100800);
            FeatureCheck.AssertOnDifference();
            Assert.That(cpu.Description, Is.EqualTo("Intel(R) Core(TM) i7-4930K CPU @ 3.40GHz"));
        }

        [Test]
        public void Corei7_6600U()
        {
            GenuineIntelCpu cpu = GetCpu("i7-6600U_SurfaceBook.xml");
            CheckSignature(0x406E3);
            FeatureCheck.Check("standard", 0xFEDAF387, 0xBFEBFBFF, 0x009C6FBB, 0x00000000, 0xBC000400);
            FeatureCheck.Check("procstate", 0x0000000F);
            FeatureCheck.Check("extended", 0x00000121, 0x2C100800);
            FeatureCheck.AssertOnDifference();
            Assert.That(cpu.Description, Is.EqualTo("Intel(R) Core(TM) i7-6600U CPU @ 2.60GHz"));
        }

        [Test]
        public void Corei7_6700K()
        {
            GenuineIntelCpu cpu = GetCpu("i7-6700K.xml");
            CheckSignature(0x506E3);
            FeatureCheck.Check("standard", 0x7FFAFBBF, 0xBFEBFBFF, 0x029C6FBF, 0x00000000, 0x9C002400);
            FeatureCheck.Check("procstate", 0x0000000F);
            FeatureCheck.Check("extended", 0x00000121, 0x2C100800);
            FeatureCheck.AssertOnDifference();
            Assert.That(cpu.Description, Is.EqualTo("Intel(R) Core(TM) i7-6700K CPU @ 4.00GHz"));
        }

        [Test]
        public void Corei7_9700()
        {
            GenuineIntelCpu cpu = GetCpu("i7-9700.xml");
            CheckSignature(0x906ED);
            FeatureCheck.Check("standard", 0x7FFAFBFF, 0xBFEBFBFF, 0x029C6FBF, 0x40000000, 0xBC000400);
            FeatureCheck.Check("procstate", 0x0000000F);
            FeatureCheck.Check("extended", 0x00000121, 0x2C100800);
            FeatureCheck.AssertOnDifference();
            Assert.That(cpu.Description, Is.EqualTo("Intel(R) Core(TM) i7-9700 CPU @ 3.00GHz"));
        }

        [Test]
        public void Corei9_10900K()
        {
            GenuineIntelCpu cpu = GetCpu("i9-10900K.xml");
            CheckSignature(0xA0655);
            FeatureCheck.Check("standard", 0x7FFAFBFF, 0xBFEBFBFF, 0x029C67AF, 0x40000008, 0xBC000400);
            FeatureCheck.Check("procstate", 0x0000000F);
            FeatureCheck.Check("extended", 0x00000121, 0x2C100000);
            FeatureCheck.AssertOnDifference();
            Assert.That(cpu.Description, Is.EqualTo("Intel(R) Core(TM) i9-10900K CPU @ 3.70GHz"));
        }
    }
}