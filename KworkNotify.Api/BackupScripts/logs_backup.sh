#!/usr/bin/env bash

LOGS_DIR="/root/build/logs"

if [ ! -d "$LOGS_DIR" ]; then
    echo "Ошибка: Директория $LOGS_DIR не найдена"
    exit 1
fi

TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
LOGS_BACKUP_NAME="logs_backup_$TIMESTAMP"

CURRENT_DATE=$(date +"%Y%m%d")

echo "Создание бэкапа логов из $LOGS_DIR..."
tar -czf "$LOGS_BACKUP_NAME.tar.gz" -C "$LOGS_DIR" .

if [ $? -eq 0 ]; then
    echo "Бэкап логов завершен: $LOGS_BACKUP_NAME.tar.gz"
    echo "Удаление исходных файлов логов, кроме логов за $CURRENT_DATE..."
    find "$LOGS_DIR" -type f -not -name "*$CURRENT_DATE.log" -exec rm -f {} \;
    if [ $? -eq 0 ]; then
        echo "Исходные файлы логов (кроме текущего дня) успешно удалены"
    else
        echo "Ошибка при удалении исходных файлов логов"
        exit 1
    fi
else
    echo "Ошибка при создании бэкапа логов"
    exit 1
fi