#!/bin/bash
# Python执行器启动脚本

cd "$(dirname "$0")"

# 激活虚拟环境
source venv/bin/activate

# 启动应用
echo "启动 Python 执行器..."
echo "访问 http://localhost:8180 查看服务状态"
python app.py
