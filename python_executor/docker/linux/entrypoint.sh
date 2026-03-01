#!/bin/bash
#
# Python执行器启动脚本
# Linux容器环境
#

set -e

# 默认值
CONFIG_PATH=${CONFIG_PATH:-/app/config/config.json}
LOG_LEVEL=${LOG_LEVEL:-INFO}
EXECUTOR_ID=${EXECUTOR_ID:-executor-linux-01}

echo "========================================"
echo "  Python执行器 容器启动"
echo "========================================"
echo ""

# 检查Python安装
echo "检查Python安装..."
python_version=$(python --version 2>&1)
echo "  [OK] Python版本: $python_version"

# 检查配置文件
echo ""
echo "检查配置文件..."
if [ -f "$CONFIG_PATH" ]; then
    echo "  [OK] 配置文件存在: $CONFIG_PATH"
else
    echo "  [WARNING] 配置文件不存在: $CONFIG_PATH"
    echo "  将使用默认配置..."
fi

# 显示环境变量
echo ""
echo "环境变量配置..."
echo "  执行器ID: $EXECUTOR_ID"
echo "  日志级别: $LOG_LEVEL"
echo "  CANoe启用: $CANOE_ENABLED"
echo "  TSMaster启用: $TSMASTER_ENABLED"

# 检查端口占用
echo ""
echo "检查端口占用..."
if lsof -Pi :8180 -sTCP:LISTEN -t >/dev/null 2>&1; then
    echo "  [WARNING] 端口8180已被占用"
else
    echo "  [OK] 端口8180可用"
fi

# 设置环境变量
export PYTHONEXECUTOR_CONFIG=$CONFIG_PATH
export PYTHONEXECUTOR_LOGLEVEL=$LOG_LEVEL

if [ "$DEBUG" = "true" ]; then
    export FLASK_DEBUG=1
    export PYTHONEXECUTOR_DEBUG=1
fi

# 切换到应用目录
cd /app

echo ""
echo "========================================"
echo "  启动Python执行器..."
echo "========================================"
echo ""

# 启动应用
exec python /app/main.py
