# Python执行器 - Windows容器镜像
# 基于Windows Server Core 2022

# 1. 基础镜像
FROM mcr.microsoft.com/windows/servercore:ltsc2022

# 2. 定义构建参数
ARG BUILD_DATE
ARG VERSION=v1.0.0

# 3. 设置工作目录
WORKDIR C:/app

# 4. 设置环境变量
ENV PYTHONUNBUFFERED=1 `
    PYTHONDONTWRITEBYTECODE=1 `
    POWERSHELL_TELEMETRY_OPTOUT=1 `
    APP_VERSION=${VERSION} `
    BUILD_DATE=${BUILD_DATE}

# 5. 安装Chocolatey包管理器
RUN powershell -Command `
    $ErrorActionPreference = 'Stop'; `
    Set-ExecutionPolicy Bypass -Scope Process -Force; `
    [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; `
    iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))

# 6. 安装Python 3.10
RUN choco install python --version=3.10.11 -y --no-progress --ignore-checksums

# 7. 刷新环境变量并安装pip包
RUN powershell -Command `
    $env:Path = [System.Environment]::GetEnvironmentVariable('Path','Machine') + ';' + [System.Environment]::GetEnvironmentVariable('Path','User'); `
    python -m pip install --upgrade pip; `
    pip install --upgrade setuptools wheel

# 8. 复制并安装依赖
COPY requirements.txt C:/app/
RUN powershell -Command `
    $env:Path = [System.Environment]::GetEnvironmentVariable('Path','Machine') + ';' + [System.Environment]::GetEnvironmentVariable('Path','User'); `
    pip install --no-cache-dir -r C:/app/requirements.txt

# 9. 复制项目文件
COPY . C:/app/

# 10. 创建日志目录
RUN powershell -New-Item -ItemType Directory -Force -Path C:/app/logs

# 11. 复制启动脚本
COPY docker/windows/entrypoint.ps1 C:/app/entrypoint.ps1

# 12. 暴露端口
EXPOSE 8180

# 13. 设置启动参数
# 使用PowerShell作为入口点
ENTRYPOINT ["powershell.exe", "-ExecutionPolicy", "Bypass", "-File", "C:/app/entrypoint.ps1"]

# 14. 镜像元数据
LABEL maintainer="Python Executor Team" `
      version="${VERSION}" `
      description="Python Test Executor for CANoe/TSMaster/TTworkbench" `
      build_date="${BUILD_DATE}"
