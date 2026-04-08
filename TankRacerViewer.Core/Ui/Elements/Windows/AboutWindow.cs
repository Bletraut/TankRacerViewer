using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

using ComposableUi;

using FastFileUnpacker;

using Microsoft.Xna.Framework;

namespace TankRacerViewer.Core
{
    public sealed class AboutWindow : WindowElement
    {
        // Static.
        private static readonly StringBuilder _stringBuilder = new();

        private static string WrapText(string text, int maxLineLength)
        {
            _stringBuilder.Clear();

            var currentLineLength = 0;

            var words = text.Split(' ');
            foreach (var word in words)
            {
                if (currentLineLength + word.Length > maxLineLength)
                {
                    currentLineLength = 0;
                    _stringBuilder.AppendLine(word);
                }

                if (word.Length > 0)
                {
                    _stringBuilder.Append(' ');
                    _stringBuilder.Append(word);
                }

                currentLineLength += word.Length;
            }

            return _stringBuilder.ToString();
        }

        // Class.
        private readonly IPlatformUrlOpener _urlOpener;

        public AboutWindow(IPlatformUrlOpener urlOpener) : base("About")
        {
            _urlOpener = urlOpener;

            var assembly = Assembly.GetExecutingAssembly();

            var version = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
            var product = assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product;
            var copyright = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright;
            var description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description;

            var repositoryUrl = assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
                .FirstOrDefault(metadata => metadata.Key == "RepositoryUrl")?.Value;

            ContentContainer.AddChild(new AlignmentElement(
                alignmentFactor: Alignment.TopLeft,
                pivot: Alignment.TopLeft,
                innerElement: new ColumnLayout(
                    sizeMainAxisToContent: true,
                    sizeCrossAxisToContent: true,
                    children: [
                        new TextElement(
                            sizeToTextWidth: true,
                            sizeToTextHeight: true,
                            text: $"{product} v{version}",
                            color: Color.Black
                        ),
                        new TextElement(
                            sizeToTextWidth: true,
                            sizeToTextHeight: true,
                            text: WrapText(description, 30),
                            color: Color.Black
                        ),
                        new TextElement(
                            sizeToTextWidth: true,
                            sizeToTextHeight: true,
                            text: copyright,
                            color: Color.Black
                        ),
                        new TextElement(
                            sizeToTextWidth: true,
                            sizeToTextHeight: true,
                            text: repositoryUrl,
                            color: Color.Black
                        )
                    ]
                )
            ));
        }
    }
}
