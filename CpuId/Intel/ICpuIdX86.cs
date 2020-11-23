﻿namespace RJCP.Diagnostics.Intel
{
    /// <summary>
    /// Information common to Intel or Intel clone CPUs.
    /// </summary>
    public interface ICpuIdX86 : ICpuId
    {
        /// <summary>
        /// Gets the processor signature, from CPUID Function 01h.
        /// </summary>
        /// <value>The processor signature.</value>
        int ProcessorSignature { get; }

        /// <summary>
        /// Gets the calculated CPU family, from CPUID Function 01h.
        /// </summary>
        /// <value>The CPU family.</value>
        int Family { get; }

        /// <summary>
        /// Gets the calculated CPU model, from CPUID Function 01h.
        /// </summary>
        /// <value>The CPU model.</value>
        int Model { get; }

        /// <summary>
        /// Gets the CPU stepping, from CPUID Function 01h.
        /// </summary>
        /// <value>The CPU stepping.</value>
        int Stepping { get; }

        /// <summary>
        /// Gets the type of the processor, from CPUID Function 01h.
        /// </summary>
        /// <value>The type of the processor.</value>
        int ProcessorType { get; }

        /// <summary>
        /// Gets the maximum possible number of APIC cores allocated to this package.
        /// </summary>
        /// <value>The maximum number of APIC cores allocated to this package.</value>
        int ApicMaxThreads { get; }

        /// <summary>
        /// Gets the APIC identifier, from CPUID Function 01h.
        /// </summary>
        /// <value>The APIC identifier.</value>
        int ApicId { get; }

        /// <summary>
        /// Access to the CPUID registers for the CPU.
        /// </summary>
        /// <value>The CPUID registers.</value>
        ICpuRegisters Registers { get; }
    }
}
