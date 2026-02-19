import * as signalR from '@microsoft/signalr';
import { getAccessToken } from './client';

const connection = new signalR.HubConnectionBuilder()
  .withUrl('http://localhost:5175/hubs/tasks', {
    accessTokenFactory: () => getAccessToken() ?? '',
  })
  .withAutomaticReconnect()
  .build();

let started = false;

export async function startConnection() {
  if (started) return;
  try {
    await connection.start();
    started = true;
    console.log('SignalR connected');
  } catch (err) {
    console.error('SignalR connection failed:', err);
    setTimeout(startConnection, 3000);
  }
}

export function onTaskClosed(callback: (taskId: string) => void) {
  connection.on('TaskClosed', callback);
  return () => connection.off('TaskClosed', callback);
}

export function onTaskRestored(callback: (taskId: string) => void) {
  connection.on('TaskRestored', callback);
  return () => connection.off('TaskRestored', callback);
}

export function onTaskUpdated(callback: (taskId: string) => void) {
  connection.on('TaskUpdated', callback);
  return () => connection.off('TaskUpdated', callback);
}
