using Autofac;
using FluentValidation;
using System;

namespace Cibc.PricingValidators.Trades
{
    public class AutofacValidatorFactory : ValidatorFactoryBase
    {
        private readonly IComponentContext container;

        public AutofacValidatorFactory(IComponentContext container)
        {
            this.container = container;
        }

        public override IValidator CreateInstance(Type validatorType)
        {
            object instance;
            if (container.TryResolve(validatorType, out instance))
            {
                var validator = instance as IValidator;
                return validator;
            }

            return null;
        }
    }

}
