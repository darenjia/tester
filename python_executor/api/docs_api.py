"""
接口文档API
提供API接口文档相关的RESTful接口
"""
from flask import Blueprint, request, jsonify, current_app
from typing import Dict, Any, List
import inspect

# 创建蓝图
docs_bp = Blueprint('docs', __name__, url_prefix='/api')


# API文档定义
API_ENDPOINTS = {
    # 任务管理接口
    "/api/tasks": {
        "GET": {
            "summary": "获取任务列表",
            "description": "获取所有任务的列表，支持分页和状态筛选",
            "params": {
                "status": {"type": "string", "required": False, "description": "任务状态筛选 (pending/running/completed/failed/cancelled/timeout)"},
                "page": {"type": "integer", "required": False, "default": 1, "description": "页码"},
                "per_page": {"type": "integer", "required": False, "default": 20, "description": "每页数量"},
                "sort_by": {"type": "string", "required": False, "default": "created_at", "description": "排序字段"},
                "sort_order": {"type": "string", "required": False, "default": "desc", "description": "排序方向 (asc/desc)"}
            },
            "response_example": {
                "success": True,
                "data": {
                    "tasks": [
                        {
                            "id": "task-uuid",
                            "name": "任务名称",
                            "status": "pending",
                            "priority": 1,
                            "task_type": "default",
                            "created_at": "2026-03-08T12:00:00"
                        }
                    ],
                    "pagination": {
                        "total": 100,
                        "page": 1,
                        "per_page": 20,
                        "total_pages": 5
                    }
                }
            }
        },
        "POST": {
            "summary": "创建新任务",
            "description": "创建一个新的执行任务",
            "params": {
                "name": {"type": "string", "required": True, "description": "任务名称"},
                "type": {"type": "string", "required": False, "default": "default", "description": "任务类型"},
                "priority": {"type": "integer", "required": False, "default": 1, "description": "优先级 (0-3)"},
                "params": {"type": "object", "required": False, "default": {}, "description": "任务参数"},
                "timeout": {"type": "integer", "required": False, "default": 3600, "description": "超时时间(秒)"},
                "delay": {"type": "integer", "required": False, "default": 0, "description": "延迟执行时间(秒)"},
                "metadata": {"type": "object", "required": False, "default": {}, "description": "额外元数据"}
            },
            "response_example": {
                "success": True,
                "message": "任务已创建并提交到队列",
                "data": {
                    "id": "task-uuid",
                    "name": "任务名称",
                    "status": "pending"
                }
            }
        }
    },
    "/api/tasks/{id}": {
        "GET": {
            "summary": "获取任务详情",
            "description": "获取指定任务的详细信息",
            "params": {
                "id": {"type": "string", "required": True, "in": "path", "description": "任务ID"}
            },
            "response_example": {
                "success": True,
                "data": {
                    "id": "task-uuid",
                    "name": "任务名称",
                    "status": "completed",
                    "result": {"output": "任务执行结果"},
                    "duration": 120.5,
                    "log_stats": {"total": 50, "INFO": 45, "ERROR": 5}
                }
            }
        },
        "DELETE": {
            "summary": "删除任务",
            "description": "删除已完成的任务",
            "params": {
                "id": {"type": "string", "required": True, "in": "path", "description": "任务ID"}
            },
            "response_example": {
                "success": True,
                "message": "任务已删除"
            }
        }
    },
    "/api/tasks/{id}/cancel": {
        "POST": {
            "summary": "取消任务",
            "description": "取消正在排队或运行中的任务",
            "params": {
                "id": {"type": "string", "required": True, "in": "path", "description": "任务ID"}
            },
            "response_example": {
                "success": True,
                "message": "任务已取消"
            }
        }
    },
    "/api/tasks/{id}/retry": {
        "POST": {
            "summary": "重试任务",
            "description": "重试失败或超时的任务",
            "params": {
                "id": {"type": "string", "required": True, "in": "path", "description": "任务ID"}
            },
            "response_example": {
                "success": True,
                "message": "任务已重试",
                "data": {"id": "new-task-uuid", "name": "任务名称"}
            }
        }
    },
    "/api/tasks/{id}/logs": {
        "GET": {
            "summary": "获取任务日志",
            "description": "获取指定任务的执行日志",
            "params": {
                "id": {"type": "string", "required": True, "in": "path", "description": "任务ID"},
                "level": {"type": "string", "required": False, "description": "日志级别筛选"},
                "page": {"type": "integer", "required": False, "default": 1, "description": "页码"},
                "per_page": {"type": "integer", "required": False, "default": 50, "description": "每页数量"}
            },
            "response_example": {
                "success": True,
                "data": {
                    "logs": [
                        {
                            "timestamp": "2026-03-08T12:00:00",
                            "level": "INFO",
                            "message": "任务开始执行"
                        }
                    ],
                    "pagination": {"total": 100, "page": 1, "per_page": 50}
                }
            }
        }
    },
    "/api/tasks/stats": {
        "GET": {
            "summary": "获取任务统计",
            "description": "获取任务队列和执行器的统计信息",
            "response_example": {
                "success": True,
                "data": {
                    "queue": {"total": 100, "pending": 10, "running": 5, "completed": 80, "failed": 5},
                    "executor": {"max_workers": 5, "running_count": 5}
                }
            }
        }
    },
    
    # 配置管理接口
    "/api/config": {
        "GET": {
            "summary": "获取所有配置",
            "description": "获取系统的所有配置信息",
            "response_example": {
                "success": True,
                "data": {
                    "http": {"port": 2887, "host": "0.0.0.0"},
                    "websocket": {"enabled": False, "port": 8080}
                }
            }
        },
        "POST": {
            "summary": "更新配置",
            "description": "批量更新系统配置",
            "params": {
                "http": {"type": "object", "required": False, "description": "HTTP配置"},
                "websocket": {"type": "object", "required": False, "description": "WebSocket配置"}
            },
            "response_example": {
                "success": True,
                "message": "配置已更新"
            }
        }
    },
    "/api/config/http": {
        "GET": {
            "summary": "获取HTTP配置",
            "description": "获取HTTP服务配置",
            "response_example": {
                "success": True,
                "data": {"port": 2887, "host": "0.0.0.0", "debug": False}
            }
        },
        "POST": {
            "summary": "更新HTTP配置",
            "description": "更新HTTP服务配置",
            "params": {
                "port": {"type": "integer", "required": False, "description": "端口号"},
                "host": {"type": "string", "required": False, "description": "主机地址"},
                "debug": {"type": "boolean", "required": False, "description": "调试模式"}
            },
            "response_example": {
                "success": True,
                "message": "HTTP配置已更新"
            }
        }
    },
    "/api/config/websocket": {
        "GET": {
            "summary": "获取WebSocket配置",
            "description": "获取WebSocket服务配置",
            "response_example": {
                "success": True,
                "data": {"enabled": False, "port": 8080, "host": "0.0.0.0"}
            }
        },
        "POST": {
            "summary": "更新WebSocket配置",
            "description": "更新WebSocket服务配置",
            "params": {
                "enabled": {"type": "boolean", "required": False, "description": "是否启用"},
                "port": {"type": "integer", "required": False, "description": "端口号"},
                "host": {"type": "string", "required": False, "description": "主机地址"}
            },
            "response_example": {
                "success": True,
                "message": "WebSocket配置已更新"
            }
        }
    },
    
    # 服务状态接口
    "/api/services/status": {
        "GET": {
            "summary": "获取服务状态",
            "description": "获取所有服务的运行状态",
            "response_example": {
                "success": True,
                "data": {
                    "executor": {"status": "running", "last_heartbeat": "2026-03-08T12:00:00"},
                    "scheduler": {"status": "running", "last_heartbeat": "2026-03-08T12:00:00"}
                }
            }
        }
    },
    "/api/system/stats": {
        "GET": {
            "summary": "获取系统统计",
            "description": "获取系统整体统计信息",
            "response_example": {
                "success": True,
                "data": {
                    "uptime": 3600,
                    "tasks": {"total": 100, "pending": 10},
                    "logs": {"total": 1000, "INFO": 900}
                }
            }
        }
    },
    "/api/system/health": {
        "GET": {
            "summary": "健康检查",
            "description": "检查系统健康状态",
            "response_example": {
                "success": True,
                "data": {
                    "status": "healthy",
                    "checks": {
                        "executor": {"status": "healthy"},
                        "scheduler": {"status": "healthy"}
                    }
                }
            }
        }
    }
}


@docs_bp.route('/docs/endpoints', methods=['GET'])
def get_all_endpoints():
    """
    获取所有接口列表
    
    查询参数:
    - category: 接口分类筛选
    """
    try:
        category = request.args.get('category')
        
        endpoints = []
        for path, methods in API_ENDPOINTS.items():
            for method, doc in methods.items():
                endpoint = {
                    "path": path,
                    "method": method,
                    "summary": doc.get("summary", ""),
                    "description": doc.get("description", ""),
                    "category": get_category(path)
                }
                endpoints.append(endpoint)
        
        # 按分类筛选
        if category:
            endpoints = [e for e in endpoints if e["category"] == category]
        
        return jsonify({
            "success": True,
            "data": {
                "endpoints": endpoints,
                "categories": list(set(e["category"] for e in endpoints))
            }
        })
        
    except Exception as e:
        return jsonify({"success": False, "message": f"获取接口列表失败: {str(e)}"}), 500


@docs_bp.route('/docs/endpoints/<path:endpoint_path>', methods=['GET'])
def get_endpoint_detail(endpoint_path: str):
    """
    获取指定接口详情
    
    路径参数:
    - endpoint_path: 接口路径（URL编码）
    
    查询参数:
    - method: HTTP方法，默认GET
    """
    try:
        method = request.args.get('method', 'GET').upper()
        
        # 解码路径
        import urllib.parse
        decoded_path = "/" + urllib.parse.unquote(endpoint_path)
        
        # 查找接口文档
        if decoded_path not in API_ENDPOINTS:
            return jsonify({"success": False, "message": "接口不存在"}), 404
        
        methods_doc = API_ENDPOINTS[decoded_path]
        
        if method not in methods_doc:
            return jsonify({"success": False, "message": f"接口不支持 {method} 方法"}), 404
        
        doc = methods_doc[method]
        
        return jsonify({
            "success": True,
            "data": {
                "path": decoded_path,
                "method": method,
                **doc
            }
        })
        
    except Exception as e:
        return jsonify({"success": False, "message": f"获取接口详情失败: {str(e)}"}), 500


@docs_bp.route('/docs/categories', methods=['GET'])
def get_categories():
    """获取接口分类列表"""
    try:
        categories = {}
        
        for path, methods in API_ENDPOINTS.items():
            category = get_category(path)
            if category not in categories:
                categories[category] = {
                    "name": category,
                    "description": get_category_description(category),
                    "count": 0
                }
            categories[category]["count"] += len(methods)
        
        return jsonify({
            "success": True,
            "data": list(categories.values())
        })
        
    except Exception as e:
        return jsonify({"success": False, "message": f"获取分类列表失败: {str(e)}"}), 500


def get_category(path: str) -> str:
    """根据路径获取接口分类"""
    if "/tasks" in path:
        return "任务管理"
    elif "/config" in path:
        return "配置管理"
    elif "/services" in path or "/system" in path:
        return "系统服务"
    elif "/logs" in path:
        return "日志管理"
    else:
        return "其他"


def get_category_description(category: str) -> str:
    """获取分类描述"""
    descriptions = {
        "任务管理": "任务创建、查询、控制相关接口",
        "配置管理": "系统配置查看和修改接口",
        "系统服务": "服务状态监控和系统信息接口",
        "日志管理": "日志查询和管理接口",
        "其他": "其他辅助接口"
    }
    return descriptions.get(category, "")
