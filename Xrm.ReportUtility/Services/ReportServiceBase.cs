using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Xrm.ReportUtility.Infrastructure;
using Xrm.ReportUtility.Interfaces;
using Xrm.ReportUtility.Models;

namespace Xrm.ReportUtility.Services
{
    public abstract class ReportServiceBase : IReportService
    {
        private readonly string[] _args;

        protected ReportServiceBase(string[] args)
        {
            _args = args;
        }

        public Report CreateReport()
        {
            var config = ParseConfig();
            var dataTransformer = DataTransformerCreator.CreateTransformer(config);

            var fileName = _args[0];
            var text = File.ReadAllText(fileName);
            var data = GetDataRows(text);
            return dataTransformer.TransformData(data);
        }

        private ReportConfig ParseConfig()
        {
            var handler = new DataFlagHandler(
                new IndexFlagHandler(
                    new VolumeFlagHandler(
                        new WeightFlagHandler(null))));
            if (!handler.Validate(_args))
                throw new ArgumentException();//Выводить варнинг желтым цветом!!!
            return new ReportConfig
            {
                WithData = _args.Contains("-data"),

                WithIndex = _args.Contains("-withIndex"),
                WithTotalVolume = _args.Contains("-withTotalVolume"),
                WithTotalWeight = _args.Contains("-withTotalWeight"),

                VolumeSum = _args.Contains("-volumeSum"),
                WeightSum = _args.Contains("-weightSum"),
                CostSum = _args.Contains("-costSum"),
                CountSum = _args.Contains("-countSum")
            };
        }

        protected abstract DataRow[] GetDataRows(string text);
    }
    
    public abstract class Handler
    {
        private readonly Handler _nextHandler;

        protected Handler(Handler nextHandler)
        {
            _nextHandler = nextHandler;
        }
        protected abstract string  FlagValue { get; }
        public virtual bool Validate(string[] args)
        {
            return _nextHandler != null && _nextHandler.Validate(args);
        }
    }

    public class DataFlagHandler : Handler
    {
        public DataFlagHandler(Handler nextHandler) : base(nextHandler)
        {
        }

        protected override string FlagValue => "-data";

        public override bool Validate(string[] args)
        {
            return args.Contains(FlagValue) || !base.Validate(args);
        }
    }

    public class IndexFlagHandler : Handler
    {
        public IndexFlagHandler(Handler nextHandler) : base(nextHandler)
        {
        }

        protected override string FlagValue => "-withIndex";

        public override bool Validate(string[] args)
        {
            return args.Contains(FlagValue) || base.Validate(args);
        }
    }

    public class VolumeFlagHandler : Handler
    {
        public VolumeFlagHandler(Handler nextHandler) : base(nextHandler)
        {
        }

        protected override string FlagValue => "-withTotalVolume";
        
        public override bool Validate(string[] args)
        {
            return args.Contains(FlagValue) || base.Validate(args);
        }
    }
    
    public class WeightFlagHandler : Handler
    {
        public WeightFlagHandler(Handler nextHandler) : base(nextHandler)
        {
        }

        protected override string FlagValue => "-withTotalWeight";
        
        public override bool Validate(string[] args)
        {
            return args.Contains(FlagValue) || base.Validate(args);
        }
    }
}
