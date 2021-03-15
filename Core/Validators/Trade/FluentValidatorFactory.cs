using Autofac;
using FluentValidation;
using System;
using Cibc.Core.Validators.Trades;

public class FluentValidatorFactory : ValidatorFactoryBase
    {
        private readonly IComponentContext container;

        public FluentValidatorFactory(IComponentContext container)
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


