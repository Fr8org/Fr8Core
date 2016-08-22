using FluentValidation;

namespace Data.Validations
{
    public class OutboundHtmlEmailValidator : OutboundEmailValidatorBase
    {
        public OutboundHtmlEmailValidator()
        {
            RuleFor(e => e.PlainText).Length(10, int.MaxValue).WithMessage("Email must have plain text of length at least 10 characters.");
            RuleFor(e => e.HTMLText).Length(10, int.MaxValue).WithMessage("Email must have HTML text of length at least 10 characters.");
        }
    }
}
