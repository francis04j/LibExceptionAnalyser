using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibExceptionAnalyser
{
    public class FormattedExceptionWriter
    {
        void WriteToFile(IEnumberable<FormattedException> formattedExceptions, string filePath);
    }
}
