# Python执行器 - Linux容器镜像
# 基于Python 3.10 Slim

# 1. 基础镜像
FROM python:3.10-slim

# 2. 定义构建参数
ARG BUILD_DATE
ARG VERSION=v1.0.0-linux

# 3. 维护者信息
LABEL maintainer="Python Executor Team" \
      version="${VERSION}" \
      description="Python Test Executor (Linux - Dev/Test)" \
      build_date="${BUILD_DATE}"

# 4. 设置工作目录
WORKDIR /app

# 5. 设置环境变量
ENV PYTHONUNBUFFERED=1 \
    PYTHONDONTWRITEBYTECODE=1 \
    DEBIAN_FRONTEND=noninteractive \
    APP_VERSION=${VERSION} \
    BUILD_DATE=${BUILD_DATE}

# 6. 安装系统依赖
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
    curl \
    ca-certificates \
    && rm -rf /var/lib/apt/lists/*

# 7. 安装Python依赖
COPY requirements.txt .
RUN pip install --no-cache-dir --upgrade pip setuptools wheel && \
    pip install --no-cache-dir -r requirements.txt

# 8. 复制项目文件
COPY . /app/

# 9. 创建非root用户
RUN useradd -m -s /bin/bash executor && \
    chown -R executor:executor /app

# 10. 创建必要的目录
RUN mkdir -p /app/logs /app/work /app/config && \
    chown -R executor:executor /app/logs /app/work /app/config

# 11. 复制启动脚本
COPY docker/linux/entrypoint.sh /app/entrypoint.sh
RUN chmod +x /app/entrypoint.sh

# 12. 暴露端口
EXPOSE 5000 5001

# 13. 健康检查
HEALTHCHECK --interval=30s --timeout=10s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:5000/health || exit 1

# 14. 切换到非root用户
USER executor

# 15. 入口点
ENTRYPOINT ["/app/entrypoint.sh"]
