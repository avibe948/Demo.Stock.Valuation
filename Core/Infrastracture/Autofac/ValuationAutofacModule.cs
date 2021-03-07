using System;
using System.Collections.Generic;
using System.Text;
using Autofac;
using Cibc.Pricing.ValuationModels;

namespace Cibc.Core.Infrastracture.Autofac
{
    public class ValuationAutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(typeof(IValuationModel<>).Assembly).AsClosedTypesOf(typeof(IValuationModel<>));
        }
    }
}
