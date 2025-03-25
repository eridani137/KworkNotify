#!/usr/bin/env bash

LOGS_DIR="/root/publish/logs"

if [ ! -d "$LOGS_DIR" ]; then
    echo "Ошибка: Директория $LOGS_DIR не найдена"
    exit 1
fi

TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
LOGS_BACKUP_NAME="logs_backup_$TIMESTAMP"

echo "Создание бэкапа логов из $LOGS_DIR..."
tar -czf "$LOGS_BACKUP_NAME.tar.gz" -C "$LOGS_DIR" .

if [ $? -eq 0 ]; then
    echo "Бэкап логов завершен: $LOGS_BACKUP_NAME.tar.gz"
    echo "Удаление исходных файлов логов..."
    rm -rf "$LOGS_DIR"/*
    if [ $? -eq 0 ]; then
        echo "Исходные файлы логов успешно удалены"
    else
        echo "Ошибка при удалении исходных файлов логов"
        exit 1
    fi
else
    echo "Ошибка при создании бэкапа логов"
    exit 1
fi
