using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DocumentProcessing
{
    public class DocumentStruct
    {
        System.Collections.Generic.Dictionary<string, DocumentAttribute> docAttributes;
        public DocumentStruct(string typeId)
        {
            docAttributes = new Dictionary<string, DocumentAttribute>();
            this.TypeId = typeId;
        }
        public string TypeId
        {
            get; private set;
        }
        public void AddAttribute(DocumentAttribute documentAttribute)
        {
            docAttributes.Add(documentAttribute.PatternId, documentAttribute);
        }
        public System.Collections.Generic.Dictionary<string, DocumentAttribute> Attributes
        {
            get
            {
                return docAttributes;
            }
        }
        public DocumentAttribute FindAttribute(string pattern)
        {
            return this.Attributes[pattern];
        }
    }
    public class DocumentStructs
    {
        System.Collections.Generic.Dictionary<string, DocumentStruct> structs;
        public DocumentStructs()
        {
            structs = new System.Collections.Generic.Dictionary<string, DocumentStruct>();
        }
        public void Add(DocumentStruct documentStruct)
        {
            structs.Add(documentStruct.TypeId, documentStruct);
        }
        public DocumentStruct Find(string typeId)
        {
            return structs[typeId];
        }
    }
    public class DocumentAttribute
    {
        public string Name { get; set; }
        public string StandardValue { get; set; }
        public string TagName { get; set; }
        public string PatternId { get; set; }
        public int Priority { get; set; }
        public string Id { get; set; }
    }
}
