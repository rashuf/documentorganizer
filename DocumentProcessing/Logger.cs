using System;
using System.Xml.Linq;
using System.IO;

namespace DocumentProcessing
{
    class Logger
    {
        DocumentStructs documentStructs;
        internal Logger(DocumentStructs docStructs)
        {
            documentStructs = docStructs;
        }
        internal System.IO.FileInfo createXml(DocumentLib.ActionObject actionObject)
        {
            string qrStr = actionObject.QRstr;
            DocumentLib.BarcodeType codeType = actionObject.CodeType;

            XDocument xdocument = new XDocument();
            XElement xelement = new XElement("Document");

            XElement xelementStatus = new XElement("Status", String.IsNullOrEmpty(actionObject.QRstr) ? "Unrecognized" : "Recognized");
            xelement.Add(xelementStatus);

            XElement xelementFileName = new XElement("FileName", actionObject.imageFile.Name);
            xelement.Add(xelementFileName);

            XElement xelementCodeType = new XElement("CodeType", codeType);
            xelement.Add(xelementCodeType);

            string typeId;
            DocumentStruct docStruct;
            if (System.String.IsNullOrEmpty(qrStr))
            {
                typeId = "";
            }
            else
            {
                XElement xelementAttributes;
                switch (actionObject.CodeType)
                {
                    case DocumentLib.BarcodeType.QR:
                        int startIndex = qrStr.IndexOf("|t="); // тип нужно точнее искать
                        int endIndex = qrStr.IndexOf("|n=");
                        typeId = qrStr.Substring(startIndex + 3, endIndex - (startIndex + 3));
                        docStruct = documentStructs.Find(typeId);
                        string[] qrAttr = qrStr.Split('|');
                        if (docStruct != null)
                        {
                            xelementAttributes = new XElement("Attributes");

                            foreach (string attr in qrAttr)
                            {
                                string patternId = attr.Substring(0, 1);
                                DocumentAttribute docAttr = new DocumentAttribute();
                                // --> в настройках аттрибутов была неправильная настройка - кириллическая "с",
                                //     она была заменена на латинскую "c", поэтому добавлена проверка
                                try
                                {
                                    docAttr = docStruct.FindAttribute(patternId);
                                }
                                catch
                                {
                                    if (patternId == "с")
                                    {
                                        patternId = "c";
                                        try
                                        {
                                            docAttr = docStruct.FindAttribute(patternId);
                                        }
                                        catch
                                        {
                                            continue;
                                        }
                                    }
                                }
                                // <--

                                xelementAttributes.Add(new XElement(docAttr.TagName, attr.Substring(2, attr.Length - 2)));
                            }
                            xelement.Add(xelementAttributes);
                        }
                        break;
                    case DocumentLib.BarcodeType.Code39:
                        xelementAttributes = new XElement("Attributes");
                        xelementAttributes.Add(new XElement("Number", qrStr));
                        xelement.Add(xelementAttributes);
                        break;
                }
            }
            string xmlFileName = String.Format(@"{0}\{1}.xml",
                                                actionObject.imageFile.DirectoryName,
                                                Path.GetFileNameWithoutExtension(actionObject.imageFile.Name));

            xdocument.Add(xelement);
            xdocument.Save(xmlFileName);

            return new FileInfo(xmlFileName);
        }
    }
}
