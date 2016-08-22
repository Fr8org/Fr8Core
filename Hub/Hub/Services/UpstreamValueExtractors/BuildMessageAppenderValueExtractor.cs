using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Hub.Services.UpstreamValueExtractors
{
    public class BuildMessageAppenderValueExtractor : UpstreamValueExtractorBase<BuildMessageAppender>
    {
        private static readonly Regex FieldPlaceholdersRegex = new Regex(@"\[.*?\]");

        protected override void ExtractUpstreamValue(BuildMessageAppender buildMessageAppender, ICrateStorage crateStorage)
        {
            var messageBodyBuilder = new StringBuilder(buildMessageAppender.Value);

            //We sort placeholders in reverse order so we can replace them starting from the last that won't break any previous match indices
            var foundPlaceholders = FieldPlaceholdersRegex.Matches(buildMessageAppender.Value).Cast<Match>().OrderByDescending(x => x.Index).ToArray();

            foreach (var placeholder in foundPlaceholders)
            {
                var fieldKey = placeholder.Value.TrimStart('[').TrimEnd(']');
                var replaceWith = GetValue(crateStorage, new FieldDTO(fieldKey));
                
                if (replaceWith != null)
                {
                    messageBodyBuilder.Replace(placeholder.Value, replaceWith.ToString(), placeholder.Index, placeholder.Value.Length);
                }
            }

            buildMessageAppender.Value = messageBodyBuilder.ToString();
        }
    }
}