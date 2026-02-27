package com.testplatform.service;

import com.testplatform.model.DeviceInfo;
import java.util.List;

/**
 * 设备管理服务接口
 * 
 * 负责设备的注册、状态监控和心跳检测
 */
public interface DeviceManagerService {
    
    /**
     * 设备注册
     * 
     * @param device 设备信息
     */
    void registerDevice(DeviceInfo device);
    
    /**
     * 设备注销
     * 
     * @param deviceId 设备ID
     */
    void unregisterDevice(String deviceId);
    
    /**
     * 处理设备心跳
     * 
     * @param deviceId 设备ID
     */
    void handleHeartbeat(String deviceId);
    
    /**
     * 获取在线设备列表
     * 
     * @return 在线设备列表
     */
    List<DeviceInfo> getOnlineDevices();
    
    /**
     * 获取设备信息
     * 
     * @param deviceId 设备ID
     * @return 设备信息
     */
    DeviceInfo getDeviceInfo(String deviceId);
    
    /**
     * 检查设备是否在线
     * 
     * @param deviceId 设备ID
     * @return 在线返回true，否则返回false
     */
    boolean isDeviceOnline(String deviceId);
    
    /**
     * 更新设备状态
     * 
     * @param deviceId 设备ID
     * @param status 状态
     */
    void updateDeviceStatus(String deviceId, String status);
}
