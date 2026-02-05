import { createContext, useContext, useState, useCallback, ReactNode } from 'react';
import { XMarkIcon, CheckCircleIcon, ExclamationCircleIcon, InformationCircleIcon, BellIcon } from '@heroicons/react/24/outline';

interface Notification {
  id: string;
  type: 'success' | 'error' | 'info' | 'warning';
  title: string;
  message?: string;
  duration?: number;
}

interface NotificationContextType {
  notifications: Notification[];
  addNotification: (notification: Omit<Notification, 'id'>) => void;
  removeNotification: (id: string) => void;
  notifySuccess: (title: string, message?: string) => void;
  notifyError: (title: string, message?: string) => void;
  notifyInfo: (title: string, message?: string) => void;
  notifyWarning: (title: string, message?: string) => void;
}

const NotificationContext = createContext<NotificationContextType | undefined>(undefined);

export function NotificationProvider({ children }: { children: ReactNode }) {
  const [notifications, setNotifications] = useState<Notification[]>([]);

  const removeNotification = useCallback((id: string) => {
    setNotifications(prev => prev.filter(n => n.id !== id));
  }, []);

  const addNotification = useCallback((notification: Omit<Notification, 'id'>) => {
    const id = Math.random().toString(36).substr(2, 9);
    const newNotification = { ...notification, id };
    
    setNotifications(prev => [...prev, newNotification]);

    // Auto remove after duration
    const duration = notification.duration ?? 5000;
    if (duration > 0) {
      setTimeout(() => {
        removeNotification(id);
      }, duration);
    }
  }, [removeNotification]);

  const notifySuccess = useCallback((title: string, message?: string) => {
    addNotification({ type: 'success', title, message });
  }, [addNotification]);

  const notifyError = useCallback((title: string, message?: string) => {
    addNotification({ type: 'error', title, message, duration: 7000 });
  }, [addNotification]);

  const notifyInfo = useCallback((title: string, message?: string) => {
    addNotification({ type: 'info', title, message });
  }, [addNotification]);

  const notifyWarning = useCallback((title: string, message?: string) => {
    addNotification({ type: 'warning', title, message, duration: 6000 });
  }, [addNotification]);

  return (
    <NotificationContext.Provider value={{ 
      notifications, 
      addNotification, 
      removeNotification,
      notifySuccess,
      notifyError,
      notifyInfo,
      notifyWarning
    }}>
      {children}
      <NotificationContainer notifications={notifications} onRemove={removeNotification} />
    </NotificationContext.Provider>
  );
}

function NotificationContainer({ notifications, onRemove }: { notifications: Notification[]; onRemove: (id: string) => void }) {
  if (notifications.length === 0) return null;

  return (
    <div className="fixed top-4 right-4 z-50 space-y-2 max-w-sm w-full">
      {notifications.map((notification) => (
        <NotificationItem key={notification.id} notification={notification} onRemove={onRemove} />
      ))}
    </div>
  );
}

function NotificationItem({ notification, onRemove }: { notification: Notification; onRemove: (id: string) => void }) {
  const icons = {
    success: <CheckCircleIcon className="h-6 w-6 text-green-400" />,
    error: <ExclamationCircleIcon className="h-6 w-6 text-red-400" />,
    info: <InformationCircleIcon className="h-6 w-6 text-blue-400" />,
    warning: <ExclamationCircleIcon className="h-6 w-6 text-yellow-400" />,
  };

  const bgColors = {
    success: 'bg-green-50 border-green-200',
    error: 'bg-red-50 border-red-200',
    info: 'bg-blue-50 border-blue-200',
    warning: 'bg-yellow-50 border-yellow-200',
  };

  return (
    <div className={`${bgColors[notification.type]} border rounded-lg shadow-lg p-4 flex items-start gap-3 animate-slide-in`}>
      {icons[notification.type]}
      <div className="flex-1 min-w-0">
        <p className="text-sm font-medium text-gray-900">{notification.title}</p>
        {notification.message && (
          <p className="mt-1 text-sm text-gray-600">{notification.message}</p>
        )}
      </div>
      <button
        onClick={() => onRemove(notification.id)}
        className="text-gray-400 hover:text-gray-600"
      >
        <XMarkIcon className="h-5 w-5" />
      </button>
      
      <style>{`
        @keyframes slide-in {
          from {
            transform: translateX(100%);
            opacity: 0;
          }
          to {
            transform: translateX(0);
            opacity: 1;
          }
        }
        .animate-slide-in {
          animation: slide-in 0.3s ease-out;
        }
      `}</style>
    </div>
  );
}

export function useNotification() {
  const context = useContext(NotificationContext);
  if (context === undefined) {
    throw new Error('useNotification must be used within a NotificationProvider');
  }
  return context;
}
