﻿using System;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using VSLangProj80;

namespace CSharp.UnionTypes.CodeGenerator
{
    [ComVisible(true)]
    [Guid("F15F1915-CDBD-46B1-9B35-72E3C04D9D0E")]
    [CodeGeneratorRegistration(typeof (SingleFileGenerator), "C# Discriminated Union Class Generator", vsContextGuids.vsContextGuidVCSProject, GeneratesDesignTimeSource = true)]
    [ProvideObject(typeof (SingleFileGenerator))]
    internal class SingleFileGenerator : IVsSingleFileGenerator, IObjectWithSite
    {
        public static string name = "csunion";
        private object site = null;

        void IObjectWithSite.SetSite(object pUnkSite)
        {
            site = pUnkSite;
        }

        void IObjectWithSite.GetSite(ref Guid riid, out IntPtr ppvSite)
        {
            if (site == null)
            {
                Marshal.ThrowExceptionForHR(VSConstants.E_NOINTERFACE);
            }

            IntPtr punk = Marshal.GetIUnknownForObject(site);
            int hr = Marshal.QueryInterface(punk, ref riid, out ppvSite);
            Marshal.Release(punk);
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(hr);
        }

        int IVsSingleFileGenerator.DefaultExtension(out string pbstrDefaultExtension)
        {
            pbstrDefaultExtension = ".g.cs";
            return VSConstants.S_OK;
        }

        int IVsSingleFileGenerator.Generate(string wszInputFilePath,
            string bstrInputFileContents,
            string wszDefaultNamespace,
            IntPtr[] rgbOutputFileContents,
            out uint pcbOutput,
            IVsGeneratorProgress pGenerateProgress)
        {
            if (bstrInputFileContents == null)
            {
                throw new ArgumentException(nameof(bstrInputFileContents));
            }

            var code = BrightSword.CSharpExtensions.DiscriminatedUnion.CodeGenerator.generate_code_for_text(bstrInputFileContents);
            var bytes = Encoding.UTF8.GetBytes(code);

            if (bytes == null)
            {
                rgbOutputFileContents[0] = IntPtr.Zero;
                pcbOutput = 0;
            }
            else
            {
                rgbOutputFileContents[0] = Marshal.AllocCoTaskMem(bytes.Length);
                Marshal.Copy(bytes, 0, rgbOutputFileContents[0], bytes.Length);
                pcbOutput = (uint)bytes.Length;
                return VSConstants.S_OK;
            }
        }
    }
}