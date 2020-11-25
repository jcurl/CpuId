﻿namespace RJCP.Diagnostics
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    public class CpuIdTest
    {
        [Test]
        public void CurrentCpuId()
        {
            ICpuIdFactory factory = new CpuIdFactory();
            ICpuId cpu = factory.Create();
            Assert.That(cpu, Is.Not.Null);
            Console.WriteLine("CPU Vendor: {0}", cpu.CpuVendor);
            Console.WriteLine("CPU Vendor Id: {0}", cpu.VendorId);
            Console.WriteLine("CPU Description: {0}", cpu.Description);

            if (cpu is Intel.ICpuIdX86 x86cpu) {
                Console.WriteLine("x86: Signature: {0:X}", x86cpu.ProcessorSignature);
                Console.WriteLine("x86: Family: {0:X}", x86cpu.Family);
                Console.WriteLine("x86: Model: {0:X}", x86cpu.Model);
                Console.WriteLine("x86: Type: {0}", x86cpu.ProcessorType);
                Console.WriteLine("x86: Stepping: {0:X}", x86cpu.Stepping);

                if (cpu is Intel.GenuineIntelCpu intelCpu) {
                    Console.WriteLine("Intel: APIC Id: {0:X}", intelCpu.ApicId);
                    Console.WriteLine("Intel: Max Threads / Package: {0}", intelCpu.ApicMaxThreads);
                }

                foreach (var reg in x86cpu.Registers) {
                    Console.WriteLine("{0:X8} {1:X8}: {2:X8} {3:X8} {4:X8} {5:X8}",
                        reg.Function, reg.SubFunction, reg.Result[0], reg.Result[1], reg.Result[2], reg.Result[3]);
                }
            }

            foreach (string feature in cpu.Features) {
                Console.WriteLine("Feature: [{0}] {1} ({2})",
                    cpu.Features[feature] ? "X" : "-", feature, cpu.Features.Description(feature));
            }
        }
    }
}
