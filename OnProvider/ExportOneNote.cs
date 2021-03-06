#region Microsoft Community License
/*****
Microsoft Community License (Ms-CL)
Published: October 12, 2006

   This license governs  use of the  accompanying software. If you use
   the  software, you accept this  license. If you  do  not accept the
   license, do not use the software.

1. Definitions

   The terms "reproduce,"    "reproduction," "derivative works,"   and
   "distribution" have  the same meaning here  as under U.S. copyright
   law.

   A  "contribution" is the  original  software, or  any additions  or
   changes to the software.

   A "contributor"  is any  person  that distributes  its contribution
   under this license.

   "Licensed  patents" are  a contributor's  patent  claims that  read
   directly on its contribution.

2. Grant of Rights

   (A) Copyright   Grant-  Subject to  the   terms  of  this  license,
   including the license conditions and limitations in section 3, each
   contributor grants   you a  non-exclusive,  worldwide, royalty-free
   copyright license to reproduce its contribution, prepare derivative
   works of its  contribution, and distribute  its contribution or any
   derivative works that you create.

   (B) Patent Grant-  Subject to the terms  of this license, including
   the   license   conditions and   limitations   in  section  3, each
   contributor grants you   a non-exclusive, worldwide,   royalty-free
   license under its licensed  patents to make,  have made, use, sell,
   offer   for   sale,  import,  and/or   otherwise   dispose  of  its
   contribution   in  the  software   or   derivative  works  of   the
   contribution in the software.

3. Conditions and Limitations

   (A) Reciprocal  Grants- For any  file you distribute  that contains
   code from the software (in source code  or binary format), you must
   provide recipients the source code  to that file  along with a copy
   of this  license,  which license  will  govern that  file.  You may
   license other  files that are  entirely  your own  work and  do not
   contain code from the software under any terms you choose.

   (B) No Trademark License- This license does not grant you rights to
   use any contributors' name, logo, or trademarks.

   (C)  If you  bring  a patent claim    against any contributor  over
   patents that you claim  are infringed by  the software, your patent
   license from such contributor to the software ends automatically.

   (D) If you distribute any portion of the  software, you must retain
   all copyright, patent, trademark,  and attribution notices that are
   present in the software.

   (E) If  you distribute any  portion of the  software in source code
   form, you may do so only under this license by including a complete
   copy of this license with your  distribution. If you distribute any
   portion  of the software in  compiled or object  code form, you may
   only do so under a license that complies with this license.

   (F) The  software is licensed  "as-is." You bear  the risk of using
   it.  The contributors  give no  express  warranties, guarantees  or
   conditions. You   may have additional  consumer  rights  under your
   local laws   which  this license  cannot   change. To   the  extent
   permitted under  your local  laws,   the contributors  exclude  the
   implied warranties of   merchantability, fitness for  a  particular
   purpose and non-infringement.


*****/
#endregion
using System;
using System.Collections.Generic;
using System.Text;
using System.Management.Automation;
using System.Collections;
using System.Xml;
using Microsoft.Office.Interop.OneNote;

namespace Microsoft.Office.OneNote.Commands
{
    /// <summary>
    /// Exports OneNote data to another format.
    /// </summary>
    [Cmdlet(VerbsData.Export, "OneNote", SupportsShouldProcess = true)]
    public class ExportOneNote : PSCmdlet
    {

        #region Parameters
        private string _psPath;

        /// <summary>
        /// One way of specifying what to convert: A PSPath naming the OneNote items.
        /// </summary>
        [Parameter(Position=0, 
            ParameterSetName="PSPath")]
        [ValidateNotNullOrEmpty()]
        public string Path
        {
            get { return _psPath; }
            set { _psPath = value; }
        }

        private string _id;

        [Parameter(ParameterSetName="OneNoteId", 
            ValueFromPipelineByPropertyName=true)]
        public string ID
        {
            get { return _id; }
            set { _id = value; }
        }

        private string _pageName;

        [Parameter(ValueFromPipeline=true, ValueFromPipelineByPropertyName=true)]
        [ValidateNotNullOrEmpty()]
        public string Name
        {
            get { return _pageName; }
            set { _pageName = value; }
        }

        private string _notebookPath;

        /// <summary>
        /// NotebookPath is the physical path to a OneNote notebook to open and convert.
        /// </summary>
        [Parameter(ParameterSetName="NotebookPath", 
            ValueFromPipelineByPropertyName=true), ValidateNotNullOrEmpty]
        public string NotebookPath
        {
            get { return _notebookPath; }
            set { _notebookPath = value; }
        }


        private string _outputName;

        /// <summary>
        /// The output file name to export to.
        /// </summary>
        [Parameter(Position=1,
                   Mandatory=true,
                   ValueFromPipelineByPropertyName=true)]
        [ValidateNotNullOrEmpty()]
        public string OutputPath
        {
            get { return _outputName; }
            set { _outputName = value; }
        }

        private string _destinationPath;

        [Parameter(ValueFromPipelineByPropertyName=true)]
        public string DestinationPath
        {
            get { return _destinationPath; }
            set { _destinationPath = value; }
        }
	

        public string[] OutputFormats = { "MHT", "PDF", "DOC", "WEB", "XPS", "RSS", "ONEPKG" };
        private string _outputFormat = "MHT";

        [Parameter()]
        public string Format
        {
            get { return _outputFormat; }
            set { _outputFormat = value; }
        }

        private DateTime _highlightChangesSince = DateTime.MaxValue;

        [Parameter(HelpMessage="If set, this parameter causes any outline elements changed after the date/time " +
                               "to be highlighted in the output.")]
        public DateTime HighlightChangesSince
        {
            get { return _highlightChangesSince; }
            set { _highlightChangesSince = value; }
        }

        private bool _force;

        [Parameter()]
        public SwitchParameter Force
        {
            get { return _force; }
            set { _force = value; }
        }

        private bool _neverCreateDirectories;

        [Parameter()]
        public SwitchParameter NeverCreateDirectories
        {
            get { return _neverCreateDirectories; }
            set { _neverCreateDirectories = value; }
        }

        private string _customResources;

        [Parameter(ValueFromPipelineByPropertyName=true)]
        public string CustomResources
        {
            get { return _customResources; }
            set { _customResources = value; }
        }
	
	
	
        #endregion

        /// <summary>
        /// Removes any invalid file name character from <c>name</c>.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private string removeInvalidFileCharacters(string name)
        {
            name = name.Replace("/", ".");
            name = name.Replace("\\", ".");
            name = name.Replace(":", ".");
            name = name.Replace("<", ".");
            name = name.Replace(">", ".");
            return name;
        }

        /// <summary>
        /// When exporting a notebook to web format, this private variable holds the pages to convert.
        /// </summary>
        private int _pagesToConvert;

        /// <summary>
        /// When converting a notebook to web format, this holds the pages that have been converted.
        /// </summary>
        private int _pagesConverted;

        private const int _conversionActivityIdentifier = (int)0xbd00;

        private ProgressRecord _progress;

        protected override void ProcessRecord( )
        {
            PublishFormat pf = PublishFormat.pfMHTML;
            bool publishWeb = false;

            //
            //  Figure out the publish format.
            //

            List<string> candidates = new List<string>( );
            Utilities.GetCandidatesForString(OutputFormats, _outputFormat, candidates);
            if ((candidates.Count > 1) || (candidates.Count == 0))
            {
                WriteError(new ErrorRecord(new ArgumentException("Format is ambiguous"),
                    "InvalidArgument",
                    ErrorCategory.InvalidArgument,
                    _outputFormat));
                return;
            }
            switch (candidates[0])
            {
                case "MHT":
                    pf = PublishFormat.pfMHTML;
                    break;

                case "PDF":
                    pf = PublishFormat.pfPDF;
                    break;

                case "DOC":
                    pf = PublishFormat.pfWord;
                    break;

                case "ONEPKG":
                    pf = PublishFormat.pfOneNotePackage;
                    break;

                case "XPS":
                    pf = PublishFormat.pfXPS;
                    break;
                    
                case "WEB":
                    publishWeb = true;
                    break;

                default:
                    throw new NotSupportedException( );
            }

            //
            //  Figure out what we're exporting.
            //

            Microsoft.Office.Interop.OneNote.ApplicationClass app = new ApplicationClass( );
            List<string> ids = new List<string>( );
            switch (ParameterSetName)
            {
                case "PSPath":
                    Utilities.GetOneNoteIdsForPsPath(this, _psPath, ids);
                    if ((ids.Count > 1) &&
                        !ShouldContinue("You specified multiple items to process, but only one will be converted. Continue?", "Convert"))
                    {
                        return;
                    }
                    if (ids.Count == 0)
                    {
                        Utilities.WriteInvalidPathError(this, _psPath);
                        return;
                    }
                    break;
                    
                case "OneNoteId":
                    ids.Add(_id);
                    break;

                case "NotebookPath":
                    string hierarchyId;
                    app.OpenHierarchy(_notebookPath, null, out hierarchyId, CreateFileType.cftNone);
                    WriteDebug("Will convert notebook: " + hierarchyId);
                    ids.Add(hierarchyId);
                    break;
            }

            //
            //  If we're doing change highlighting, then we need to know where the Unfiled Notes section
            //  is. We copy the page to that section, update the page XML to do the highlighting, etc.
            //

            if (_highlightChangesSince != DateTime.MaxValue)
            {
                throw new NotImplementedException( );
            }
            
            int progressSalt = 0;
            foreach (string id in ids)
            {
                if (!ShouldProcess(id))
                {
                    continue;
                }
                string output = _outputName;
                if (!String.IsNullOrEmpty(_pageName))
                {
                    output = System.IO.Path.Combine(output, removeInvalidFileCharacters(_pageName));
                    if (candidates[0] != "WEB")
                    {
                        output += "." + candidates[0];
                    }
                }
                WriteDebug("Exporting page " + id + " to " + output);
                if (publishWeb)
                {
                    OneNoteToRSS.OneNoteToRSS converter = new OneNoteToRSS.OneNoteToRSS( );
                    converter.OutputFolder = output;

                    if (!System.IO.Directory.Exists(output))
                    {
                        if (ShouldContinue("Directory '" + output + "' does not exist. Create it?",
                                "Create Directory?",
                                ref _force,
                                ref _neverCreateDirectories))
                        {
                            System.IO.Directory.CreateDirectory(output);
                        } else
                        {
                            WriteError(new ErrorRecord(new System.IO.DirectoryNotFoundException("'" + output + "' not found."),
                                "DirectoryNotFound",
                                ErrorCategory.ObjectNotFound,
                                output));
                            continue;
                        }
                    }

                    //
                    //  Need to get the notebook path corresponding to this ID.
                    //

                    string hierarchy;
                    app.GetHierarchy(id, HierarchyScope.hsSelf, out hierarchy);
                    XmlDocument hierarchyDoc = new XmlDocument( );
                    hierarchyDoc.LoadXml(hierarchy);
                    if (hierarchyDoc.DocumentElement.LocalName != "Notebook")
                    {
                        WriteError(new ErrorRecord(new ArgumentException("Only Notebooks may be converted to websites."),
                            "InvalidArgument",
                            ErrorCategory.InvalidArgument,
                            id));
                        continue;
                    }
                    string notebookPath = hierarchyDoc.DocumentElement.Attributes["path"].Value;
                    progressSalt++;
                    _progress = new ProgressRecord(_conversionActivityIdentifier + progressSalt, "Converting " + notebookPath + " To Web Notebook", "Converting");
                    WriteDebug("Outputting " + notebookPath + " to " + output);
                    converter.NotebookFolder = notebookPath;
                    if (String.IsNullOrEmpty(_customResources))
                    {
                        converter.ResourceFolder = OneNoteToRSS.OneNoteToRSS.DefaultResourceFolder;
                    } else
                    {
                        converter.ResourceFolder = _customResources;
                    }
                    converter.PageConverted += new EventHandler<OneNoteToRSS.PageConversionEventArgs>(converter_PageConverted);
                    converter.PreSectionConverted += new EventHandler<OneNoteToRSS.SectionConversionEventArgs>(converter_PreSectionConverted);
                    converter.PageSkipped += new EventHandler<OneNoteToRSS.PageConversionEventArgs>(converter_PageSkipped);
                    converter.OpenOneNoteHierarchy( );
                    _pagesToConvert = converter.PageCount;
                    _pagesConverted = 0;
                    converter.Convert( );
                    OneNoteToRSS.FolderCopier.Copy(converter.ResourceFolder, output);
                    WriteObject(new ExportResults(id, candidates[0], output, _destinationPath));
                    
                } else
                {
                    try
                    {
                        app.Publish(id, output, pf, "");
                        WriteObject(new ExportResults(id, candidates[0], output));
                    } catch (Exception e)
                    {
                        WriteError(new ErrorRecord(e, "Exception", ErrorCategory.NotSpecified, id));
                    }
                }
            }
        }

        void converter_PageSkipped(object sender, OneNoteToRSS.PageConversionEventArgs e)
        {
            _pagesConverted++;
            _progress.StatusDescription = "Skipping page: " + e.PageName + " (" + _pagesConverted + "/" + _pagesToConvert + ")";
            _progress.PercentComplete = (int)Math.Floor(100.0 * (double)_pagesConverted / (double)_pagesToConvert);
            WriteProgress(_progress);
        }

        void converter_PreSectionConverted(object sender, OneNoteToRSS.SectionConversionEventArgs e)
        {
            WriteVerbose("Converting section: " + e.SectionName);
        }

        void converter_PageConverted(object sender, OneNoteToRSS.PageConversionEventArgs e)
        {
            _pagesConverted++;
            _progress.StatusDescription = "Converting page: " + e.PageName + " (" + _pagesConverted + "/" + _pagesToConvert + ")";
            _progress.PercentComplete = (int)Math.Floor(100.0 * (double)_pagesConverted / (double)_pagesToConvert);
            WriteProgress(_progress);
        }
    }

    public class ExportResults
    {
        private string _id;

        public string ID
        {
            get { return _id; }
            set { _id = value; }
        }

        private string _format;

        public string Format
        {
            get { return _format; }
            set { _format = value; }
        }
	
	
        private string _exportedFile;

        public string ExportedFile
        {
            get { return _exportedFile; }
            set { _exportedFile = value; }
        }

        private string _destinationPath;

        public string DestinationPath
        {
            get { return _destinationPath; }
            set { _destinationPath = value; }
        }
	

        public ExportResults(string id, string format, string exportedFile, string destinationPath)
        {
            _id = id;
            _format = format;
            _exportedFile = exportedFile;
            _destinationPath = destinationPath;
        }

        public ExportResults(string id, string format, string exportedFile)
        {
            _id = id;
            _format = format;
            _exportedFile = exportedFile;
            _destinationPath = null;
        }
    }
}
