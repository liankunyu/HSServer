using System;
using System.Data;
using System.Xml;
using System.Collections;
using System.IO;
using System.Windows.Forms;

/// <summary>
/// XML操作：
/// 参照手册http://www.w3school.com.cn/xpath/xpath_operators.asp
/// </summary>
/// 
namespace HS
{

    public class XMLHealper
    {
        #region 构造函数
        /// <summary>
        /// 创建XMLHelper类并打开XML文档
        /// </summary>
        /// <param name="strpath">提供一个XML文件路径</param>
        public XMLHealper(string strpath)
        {
            FilePath = strpath;
            OpenXML();
        }
        #endregion

        #region 对象定义

        private XmlDocument xmlDoc = new XmlDocument();
        XmlNode xmlnode;
        XmlElement xmlelem;

        #endregion

        #region 属性定义

        private string errorMess;
        public string ErrorMess
        {
            get { return errorMess; }
            set { errorMess = value; }
        }

        private string filePath;
        public string FilePath
        {
            set { filePath = value; }
            get { return filePath; }
        }

        #endregion

        #region 创建XML操作对象
        /// <summary>
        /// 创建XML操作对象
        /// </summary>
        public void OpenXML()
        {
            try
            {
                if (!string.IsNullOrEmpty(FilePath))
                {
                    xmlDoc.Load(filePath);
                }
                else
                {
                    FilePath = string.Format(@"E:\log4net.xml"); //默认路径
                    xmlDoc.Load(filePath);
                }
            }
            catch
            {
            }
        }
        #endregion

        #region 创建Xml 文档
        /// <summary>
        /// 创建一个带有根节点的Xml 文件
        /// </summary>
        /// <param name="FileName">Xml 文件名称</param>
        /// <param name="rootName">根节点名称</param>
        /// <param name="Encode">编码方式:gb2312，UTF-8 等常见的</param>
        /// <param name="DirPath">保存的目录路径</param>
        /// <returns></returns>
        public bool CreatexmlDocument(string FileName, string rootName, string Encode)
        {
            try
            {
                XmlDeclaration xmldecl;
                xmldecl = xmlDoc.CreateXmlDeclaration("1.0", Encode, null);
                xmlDoc.AppendChild(xmldecl);
                xmlelem = xmlDoc.CreateElement("", rootName, "");
                xmlDoc.AppendChild(xmlelem);
                xmlDoc.Save(FileName);
                return true;
            }
            catch 
            {
                return false;
            }
        }
        #endregion

        //获取值

        #region 得到表
        /// <summary>
        /// 得到表
        /// </summary>
        /// <returns></returns>
        public DataView GetData(string sinNode)
        {
            DataSet ds = new DataSet();
            StringReader read = new StringReader(xmlDoc.SelectSingleNode(sinNode).OuterXml);
            ds.ReadXml(read);
            return ds.Tables[0].DefaultView;
        }
        #endregion


        #region 读取指定节点的指定属性值
        /// <summary>
        /// 功能:
        /// 读取指定节点的指定属性值
        /// </summary>
        /// <param name="strNode">节点名称(相对路径：//+节点名称)</param>
        /// <param name="strAttribute">此节点的属性</param>
        /// <returns></returns>
        public string GetNodeAttributeValue(string strNode, string strAttribute)
        {
            string strReturn = "";
            try
            {
                //根据指定路径获取节点
                XmlNode xmlNode = xmlDoc.SelectSingleNode(strNode);
                //获取节点的属性，并循环取出需要的属性值
                XmlAttributeCollection xmlAttr = xmlNode.Attributes;

                for (int i = 0; i < xmlAttr.Count; i++)
                {
                    if (xmlAttr.Item(i).Name == strAttribute)
                    {
                        strReturn = xmlAttr.Item(i).Value;
                    }
                }
            }
            catch (XmlException xmle)
            {
                throw xmle;
            }
            return strReturn;
        }
        #endregion

        #region 读取指定节点的值
        /// <summary>
        /// 功能:
        /// 读取指定节点的值
        /// </summary>
        /// <param name="strNode">路径节点名称</param>
        /// <returns></returns>
        public string PathGetNodeValue(string strNode)
        {
            string strReturn = String.Empty;
            try
            {
                //根据路径获取节点
                XmlNode xmlNode = xmlDoc.SelectSingleNode(strNode);
                strReturn = xmlNode.InnerText;
            }
            catch (XmlException xmle)
            {
                MessageBox.Show(xmle.Message);
            }
            return strReturn;
        }
        #endregion

        #region 获取XML文件的根元素
        /// <summary>
        /// 获取XML文件的根元素
        /// </summary>
        public XmlNode GetXmlRoot()
        {
            return xmlDoc.DocumentElement;
        }
        #endregion

       
        //添加或插入

        #region 设置节点值
        /// <summary>
        /// 功能:
        /// 设置节点值
        /// </summary>
        /// <param name="strNode">节点的名称</param>
        /// <param name="newValue">节点值</param>
        public void SetXmlNodeValue(string xmlNodePath, string xmlNodeValue)
        {
            try
            {
                //根据指定路径获取节点
                XmlNode xmlNode = xmlDoc.SelectSingleNode(xmlNodePath);
                //设置节点值
                xmlNode.InnerText = xmlNodeValue;
            }
            catch (XmlException xmle)
            {
                throw xmle;
            }
        }
        #endregion

        #region 添加父节点

        /// <summary>
        /// 在根节点下添加父节点
        /// </summary>
        public void AddParentNode(string parentNode)
        {
            XmlNode root = GetXmlRoot();
            XmlNode parentXmlNode = xmlDoc.CreateElement(parentNode);

            root.AppendChild(parentXmlNode);
        }
        #endregion

        #region 向一个已经存在的父节点中插入一个子节点并赋值
        /// <summary>
        /// 向一个已经存在的父节点中插入一个子节点并赋值
        /// </summary>
        public void AddChildNode(string parentNodePath, string childNodePath,string value)
        {
            XmlNode parentXmlNode = xmlDoc.SelectSingleNode(parentNodePath);
            XmlNode childXmlNode = xmlDoc.CreateElement(childNodePath);
            childXmlNode.InnerText = value;
            parentXmlNode.AppendChild(childXmlNode);
        }
        #endregion

        #region 向一个节点添加属性
        /// <summary>
        /// 向一个节点添加属性
        /// </summary>
        public void AddAttribute(string NodePath, string NodeAttribute)
        {
            XmlAttribute nodeAttribute = xmlDoc.CreateAttribute(NodeAttribute);
            XmlNode nodePath = xmlDoc.SelectSingleNode(NodePath);
            nodePath.Attributes.Append(nodeAttribute);
        }
        #endregion
        
        #region 设置节点属性
        /// <summary>
        /// 设置节点属性
        /// </summary>
        /// <param name="xe">节点所处的Element</param>
        /// <param name="htAttribute">节点属性，Key 代表属性名称，Value 代表属性值</param>
        public void SetAttributes(string NodePath, Hashtable htAttribute)
        {
            XmlNode nodePath = xmlDoc.SelectSingleNode(NodePath);
            XmlElement xe = (XmlElement)nodePath;
            foreach (DictionaryEntry de in htAttribute)
            {
                xe.SetAttribute(de.Key.ToString(), de.Value.ToString());
            }
        }
        #endregion

        

        //更新

        #region 设置节点的属性值
        /// <summary>
        /// 功能:
        /// 设置节点的属性值
        /// </summary>
        /// <param name="xmlNodePath">节点名称</param>
        /// <param name="xmlNodeAttribute">属性名称</param>
        /// <param name="xmlNodeAttributeValue">属性值</param>
        public void SetXmlNodeValue(string xmlNodePath, string xmlNodeAttribute, string xmlNodeAttributeValue)
        {
            try
            {
                //根据指定路径获取节点
                XmlNode xmlNode = xmlDoc.SelectSingleNode(xmlNodePath);

                //获取节点的属性，并循环取出需要的属性值
                XmlAttributeCollection xmlAttr = xmlNode.Attributes;
                for (int i = 0; i < xmlAttr.Count; i++)
                {
                    if (xmlAttr.Item(i).Name == xmlNodeAttribute)
                    {
                        xmlAttr.Item(i).Value = xmlNodeAttributeValue;
                        break;
                    }
                }
            }
            catch (XmlException xmle)
            {
                throw xmle;
            }
        }

        #endregion

        #region 更新节点
        /// <summary>
        /// 更新节点
        /// </summary>
        /// <param name="fatherNode">需要更新节点的上级节点</param>
        /// <param name="htAtt">需要更新的属性表，Key 代表需要更新的属性，Value 代表更新后的值</param>
        /// <param name="htSubNode">需要更新的子节点的属性表，Key 代表需要更新的子节点名字Name,Value 代表更新后的值InnerText</param>
        /// <returns>返回真为更新成功，否则失败</returns>
        public bool UpdateNode(string fatherNode, Hashtable htAtt, Hashtable htSubNode)
        {
            try
            {
                xmlDoc = new XmlDocument();
                xmlDoc.Load(FilePath);
                XmlNodeList root = xmlDoc.SelectSingleNode(fatherNode).ChildNodes;
                UpdateNodes(root, htAtt, htSubNode);
                xmlDoc.Save(FilePath);
                return true;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        #endregion

        #region 更新节点属性和子节点InnerText 值
        /// <summary>
        /// 更新节点属性和子节点InnerText 值
        /// </summary>
        /// <param name="root">根节点名字</param>
        /// <param name="htAtt">需要更改的属性名称和值</param>
        /// <param name="htSubNode">需要更改InnerText 的子节点名字和值</param>
        public void UpdateNodes(XmlNodeList root, Hashtable htAtt, Hashtable htSubNode)
        {
            foreach (XmlNode xn in root)
            {
                xmlelem = (XmlElement)xn;
                if (xmlelem.HasAttributes)//如果节点如属性，则先更改它的属性
                {
                    foreach (DictionaryEntry de in htAtt)//遍历属性哈希表
                    {
                        if (xmlelem.HasAttribute(de.Key.ToString()))//如果节点有需要更改的属性
                        {
                            xmlelem.SetAttribute(de.Key.ToString(), de.Value.ToString());//则把哈希表中相应的值Value 赋给此属性Key
                        }
                    }
                }
                if (xmlelem.HasChildNodes)//如果有子节点，则修改其子节点的InnerText
                {
                    XmlNodeList xnl = xmlelem.ChildNodes;
                    foreach (XmlNode xn1 in xnl)
                    {
                        XmlElement xe = (XmlElement)xn1;
                        foreach (DictionaryEntry de in htSubNode)
                        {
                            if (xe.Name == de.Key.ToString())//htSubNode 中的key 存储了需要更改的节点名称，
                            {
                                xe.InnerText = de.Value.ToString();//htSubNode中的Value存储了Key 节点更新后的数据
                            }
                        }
                    }
                }
            }
        }
        #endregion

        //删除

        #region 删除一个节点的属性
        /// <summary>
        /// 删除一个节点的属性
        /// </summary>
        public void DeleteAttribute(string NodePath, string NodeAttribute, string NodeAttributeValue)
        {
            XmlNodeList nodePath = xmlDoc.SelectSingleNode(NodePath).ChildNodes;

            foreach (XmlNode xn in nodePath)
            {
                XmlElement xe = (XmlElement)xn;

                if (xe.GetAttribute(NodeAttribute) == NodeAttributeValue)
                {
                    xe.RemoveAttribute(NodeAttribute);//删除属性
                }
            }
        }

        #endregion

        #region 删除一个节点
        /// <summary>
        /// 删除一个节点
        /// </summary>
        public void DeleteXmlNode(string tempXmlNode)
        {
            XmlNode xmlNodePath = xmlDoc.SelectSingleNode(tempXmlNode);
            xmlNodePath.ParentNode.RemoveChild(xmlNodePath);
        }

        #endregion

        #region 删除指定节点下的子节点
        /// <summary>
        /// 删除指定节点下的子节点
        /// </summary>
        /// <param name="fatherNode">制定节点</param>
        /// <returns>返回真为更新成功，否则失败</returns>
        public bool DeleteNodes(string fatherNode)
        {
            try
            {
                xmlDoc = new XmlDocument();
                xmlDoc.Load(FilePath);
                xmlnode = xmlDoc.SelectSingleNode(fatherNode);
                xmlnode.RemoveAll();
                return true;
            }
            catch (XmlException xe)
            {
                throw new XmlException(xe.Message);
            }
        }
        #endregion

        //内部函数与保存

        #region 私有函数

        private string functionReturn(XmlNodeList xmlList, int i, string nodeName)
        {
            string node = xmlList[i].ToString();
            string rusultNode = "";
            for (int j = 0; j < i; j++)
            {
                if (node == nodeName)
                {
                    rusultNode = node.ToString();
                }
                else
                {
                    if (xmlList[j].HasChildNodes)
                    {
                        functionReturn(xmlList, j, nodeName);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return rusultNode;


        }

        #endregion

        #region 保存XML文件
        /// <summary>
        /// 功能: 
        /// 保存XML文件
        /// 
        /// </summary>
        public void SavexmlDocument()
        {
            try
            {
                xmlDoc.Save(FilePath);
            }
            catch (XmlException xmle)
            {
                throw xmle;
            }
        }
        /// <summary>
        /// 功能: 保存XML文件
        /// </summary>
        /// <param name="tempXMLFilePath"></param>
        public void SavexmlDocument(string tempXMLFilePath)
        {
            try
            {
                xmlDoc.Save(tempXMLFilePath);
            }
            catch (XmlException xmle)
            {
                throw xmle;
            }
        }
        #endregion
    }
}

