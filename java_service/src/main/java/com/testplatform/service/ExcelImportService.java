package com.testplatform.service;

import com.testplatform.model.TestConfig;
import com.testplatform.model.ImportResult;
import com.testplatform.model.ValidationResult;
import org.springframework.web.multipart.MultipartFile;

import java.io.InputStream;
import java.util.List;

/**
 * Excel导入服务接口
 * 
 * 负责Excel文件的解析、字段分解和数据校验
 */
public interface ExcelImportService {
    
    /**
     * 导入Excel文件
     * 
     * @param file Excel文件
     * @return 导入结果
     */
    ImportResult importExcel(MultipartFile file);
    
    /**
     * 解析Excel数据
     * 
     * @param inputStream 文件输入流
     * @return 测试配置列表
     */
    List<TestConfig> parseExcelData(InputStream inputStream);
    
    /**
     * 验证Excel数据
     * 
     * @param configs 测试配置列表
     * @return 验证结果
     */
    ValidationResult validateExcelData(List<TestConfig> configs);
    
    /**
     * 分解复合字段
     * 
     * @param value 字段值
     * @param delimiter 分隔符
     * @return 分解后的值列表
     */
    List<String> splitField(String value, String delimiter);
    
    /**
     * 获取Excel模板
     * 
     * @return 模板文件字节数组
     */
    byte[] getExcelTemplate();
    
    /**
     * 导出错误数据
     * 
     * @param configs 包含错误信息的配置列表
     * @return 错误数据文件字节数组
     */
    byte[] exportErrorData(List<TestConfig> configs);
}
