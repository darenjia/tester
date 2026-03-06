#!/bin/bash
#
# Mac打包脚本
# 将Python执行器打包为macOS可执行文件
#
# 使用方法:
#   chmod +x build_exe_mac.sh
#   ./build_exe_mac.sh
#

set -e  # 遇到错误立即退出

echo "============================================"
echo "   Python执行器 - Mac打包工具"
echo "============================================"
echo ""

# 颜色定义
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# 检查Python
echo "[1/5] 检查Python环境..."
if ! command -v python3 &> /dev/null; then
    echo -e "${RED}[错误] 未检测到Python3，请先安装Python 3.8或更高版本${NC}"
    exit 1
fi

PYTHON_VERSION=$(python3 --version)
echo "检测到: $PYTHON_VERSION"
echo ""

# 检查并安装pyinstaller
echo "[2/5] 检查PyInstaller..."
if ! python3 -c "import PyInstaller" 2>/dev/null; then
    echo "PyInstaller未安装，正在安装..."
    pip3 install pyinstaller
    if [ $? -ne 0 ]; then
        echo -e "${RED}[错误] PyInstaller安装失败${NC}"
        exit 1
    fi
else
    echo "PyInstaller已安装"
fi
echo ""

# 安装项目依赖
echo "[3/5] 安装项目依赖..."
pip3 install -r requirements.txt
if [ $? -ne 0 ]; then
    echo -e "${YELLOW}[警告] 部分依赖安装失败，继续打包...${NC}"
fi
echo ""

# 清理旧构建文件
echo "[4/5] 清理旧构建文件..."
rm -rf build dist __pycache__
find . -name "*.pyc" -delete
find . -name "__pycache__" -type d -delete
rm -f *.spec
echo "清理完成"
echo ""

# 执行打包
echo "[5/5] 开始打包..."
echo "使用配置: PythonExecutor_mac.spec"
echo ""

pyinstaller PythonExecutor_mac.spec

if [ $? -ne 0 ]; then
    echo ""
    echo -e "${RED}[错误] 打包失败！${NC}"
    echo "请检查错误信息并修复问题"
    exit 1
fi

echo ""
echo "============================================"
echo -e "${GREEN}   打包成功！${NC}"
echo "============================================"
echo ""
echo "可执行文件位置:"
echo "   dist/PythonExecutor"
echo ""
echo "App Bundle位置 (如启用):"
echo "   dist/PythonExecutor.app"
echo ""
echo "使用说明:"
echo "   1. 命令行运行: ./dist/PythonExecutor"
echo "   2. 服务将在 http://localhost:5000 启动"
echo ""
echo "注意:"
echo "   - 首次运行可能需要几秒钟初始化"
echo "   - 如需调试，请使用命令行运行查看日志"
echo "   - 配置文件位于 ~/.python_executor/config/"
echo ""
echo "============================================"
