using Data.Entities;
using FluentValidation;

namespace Data.Validations
{
    public class EnvelopeValidator : AbstractValidator<EnvelopeDO>
    {
        private readonly OutboundTemplatedEmailValidator _templatedEmailValidator;
        private readonly OutboundHtmlEmailValidator _htmlEmailValidator;

        public EnvelopeValidator()
        {
            _templatedEmailValidator = new OutboundTemplatedEmailValidator();
            _htmlEmailValidator = new OutboundHtmlEmailValidator();
        }

        public override FluentValidation.Results.ValidationResult Validate(EnvelopeDO instance)
        {
            var ruleBuilder = this.RuleFor(e => e.Email);
            var options = !string.IsNullOrEmpty(instance.TemplateName)
                              ? ruleBuilder.SetValidator(_templatedEmailValidator)
                              : ruleBuilder.SetValidator(_htmlEmailValidator);
            options.WithMessage("Envelope Email validation failed.");
            return base.Validate(instance);
        }
    }
}
