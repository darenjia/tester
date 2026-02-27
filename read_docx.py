import zipfile
import xml.etree.ElementTree as ET

def read_docx_content(docx_path):
    """读取DOCX文档内容"""
    content = []
    
    try:
        with zipfile.ZipFile(docx_path, 'r') as zf:
            # 检查word/document.xml文件是否存在
            if 'word/document.xml' not in zf.namelist():
                print("Error: word/document.xml not found in DOCX file")
                return content
            
            # 读取并解析document.xml
            with zf.open('word/document.xml') as f:
                tree = ET.parse(f)
                root = tree.getroot()
                
                # 定义命名空间
                namespaces = {'w': 'http://schemas.openxmlformats.org/wordprocessingml/2006/main'}
                
                # 查找所有段落
                paragraphs = root.findall('.//w:p', namespaces)
                
                for p in paragraphs:
                    # 查找段落中的所有文本节点
                    texts = p.findall('.//w:t', namespaces)
                    if texts:
                        para_text = ''.join(t.text if t.text else '' for t in texts)
                        if para_text.strip():
                            content.append(para_text)
                            
    except Exception as e:
        print(f"Error reading DOCX file: {e}")
    
    return content

if __name__ == "__main__":
    docx_path = "d:\\Deng\\can_test\\TDM2.0与网络测试平台交互文档 (1)(1)(1).docx"
    content = read_docx_content(docx_path)
    
    print("\n".join(content))
