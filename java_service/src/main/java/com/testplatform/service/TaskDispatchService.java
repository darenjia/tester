package com.testplatform.service;

import com.testplatform.model.Task;
import com.testplatform.model.TaskDispatchResult;
import com.testplatform.model.TaskStatus;

/**
 * 任务调度服务接口
 * 
 * 负责任务的下发、取消和状态查询
 */
public interface TaskDispatchService {
    
    /**
     * 下发任务到指定设备
     * 
     * @param task 任务信息
     * @param deviceId 设备ID
     * @return 任务下发结果
     */
    TaskDispatchResult dispatchTask(Task task, String deviceId);
    
    /**
     * 取消正在执行的任务
     * 
     * @param taskId 任务ID
     * @return 取消成功返回true，否则返回false
     */
    boolean cancelTask(String taskId);
    
    /**
     * 查询任务执行状态
     * 
     * @param taskId 任务ID
     * @return 任务状态
     */
    TaskStatus getTaskStatus(String taskId);
    
    /**
     * 处理任务状态更新（来自Python执行器）
     * 
     * @param taskId 任务ID
     * @param status 状态
     * @param error 错误信息（如果有）
     */
    void handleStatusUpdate(String taskId, String status, String error);
    
    /**
     * 处理任务进度更新（来自Python执行器）
     * 
     * @param taskId 任务ID
     * @param progress 进度信息
     */
    void handleProgressUpdate(String taskId, Object progress);
    
    /**
     * 处理任务执行结果（来自Python执行器）
     * 
     * @param taskId 任务ID
     * @param results 执行结果
     */
    void handleTaskResult(String taskId, Object results);
}
