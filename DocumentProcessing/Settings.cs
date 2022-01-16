using System.Xml.Linq;

namespace DocumentProcessing
{
    public class Settings
    {
        DocumentStructs documentStructs;
        public string OrigSubFolder
        {
            get; internal set;
        }
        public string RecognizedSubFolder
        {
            get; internal set;
        }
        public string UnrecognizedSubFolder
        {
            get; internal set;
        }
        public string TrashSubFolder
        {
            get; internal set;
        }
        public string StructsFileName
        {
            get; internal set;
        }
        public string SupportedFiles
        {
            get; internal set;
        }
        //public bool IsSaveResizedImage
        //{
        //    get; internal set;
        //}
        public DocumentStructs DocumentStructs
        {
            get
            {
                if (documentStructs == null)
                {
                    this.InitDocumentStructs();
                }
                return documentStructs;
            }
        }
        internal void InitDocumentStructs()
        {
            documentStructs = new DocumentStructs();

            System.Xml.Linq.XDocument xdocStructs = XDocument.Load(this.StructsFileName);
            foreach (System.Xml.Linq.XElement xdocStruct in xdocStructs.Element("DocumentStructs").Elements("DocumentStruct"))
            {
                DocumentStruct documentStruct = new DocumentStruct(xdocStruct.Attribute("TypeId").Value);
                foreach (System.Xml.Linq.XElement xdocAttr in xdocStruct.Elements("Attribute"))
                {
                    DocumentAttribute documentAttribute = new DocumentAttribute();
                    documentAttribute.Id = xdocAttr.Attribute("Id").Value;
                    documentAttribute.TagName = xdocAttr.Attribute("TagName").Value;
                    documentAttribute.PatternId = xdocAttr.Attribute("PatternId").Value;
                    documentAttribute.Priority = System.Convert.ToInt32(xdocAttr.Attribute("Priority").Value);
                    documentAttribute.StandardValue = xdocAttr.Attribute("StandardValue").Value;
                    documentAttribute.Name = xdocAttr.Attribute("Name").Value;

                    documentStruct.AddAttribute(documentAttribute);
                }
                documentStructs.Add(documentStruct);
            }
        }
    }
}
