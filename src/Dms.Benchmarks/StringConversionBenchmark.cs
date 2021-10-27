using System;
using System.Text;
using BenchmarkDotNet.Attributes;
using Dms.Common.Helpers;

namespace Dms.Benchmarks
{
    public class StringConversionBenchmark
    {

        private byte[] _sampleBinary;
        private string _sampleOriginal;

        public StringConversionBenchmark()
        {
            _sampleOriginal = Guid.NewGuid().ToString();
            _sampleBinary = Encoding.ASCII.GetBytes(_sampleOriginal);
        }
        
        [Benchmark]
        public void ConversionUsingEncoding()
        {
            var str = Encoding.ASCII.GetString(_sampleBinary);
            
            if (str != _sampleOriginal)
            {
                throw new Exception("String conversion fails");
            }
        }

        [Benchmark]
        public void ConversionUsingBinaryConverter()
        {
            var str = BinaryToStringConverter.Shared.GetStringForBytes(_sampleBinary);
            
            if (str != _sampleOriginal)
            {
                throw new Exception("String conversion fails");
            }
        }
    }
}