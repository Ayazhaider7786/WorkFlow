import { ChatBubbleLeftIcon, PaperClipIcon } from '@heroicons/react/24/outline';

interface TicketCardProps {
  itemKey: string;
  title: string;
  type?: number;
  priority: number;
  assignedToName?: string;
  estimatedHours?: number;
  commentCount?: number;
  attachmentCount?: number;
  childCount?: number;
  onClick: () => void;
  onDragStart: (e: React.DragEvent) => void;
}

const PriorityLabels: Record<number, string> = { 0: 'Low', 1: 'Med', 2: 'High', 3: 'Crit' };
const TypeIcons: Record<number, string> = { 0: '🎯', 1: '🔷', 2: '📖', 3: '✓', 4: '🐛', 5: '📌' };
const TypeColors: Record<number, string> = { 
  0: 'bg-purple-100 text-purple-700', 
  1: 'bg-blue-100 text-blue-700', 
  2: 'bg-green-100 text-green-700', 
  3: 'bg-gray-100 text-gray-700', 
  4: 'bg-red-100 text-red-700', 
  5: 'bg-yellow-100 text-yellow-700' 
};

export default function TicketCard({
  itemKey,
  title,
  type = 3,
  priority,
  assignedToName,
  estimatedHours,
  commentCount = 0,
  attachmentCount = 0,
  childCount = 0,
  onClick,
  onDragStart,
}: TicketCardProps) {
  const getPriorityColor = (p: number) => {
    switch (p) {
      case 3: return 'bg-red-500';
      case 2: return 'bg-orange-500';
      case 1: return 'bg-blue-500';
      default: return 'bg-green-500';
    }
  };

  return (
    <div
      draggable
      onDragStart={onDragStart}
      onClick={onClick}
      className="bg-white rounded-md shadow-sm border border-gray-200 p-3 cursor-pointer hover:shadow-md transition-shadow"
    >
      {/* Header row */}
      <div className="flex items-center justify-between mb-1.5">
        <div className="flex items-center gap-1.5">
          <span className={`text-xs px-1.5 py-0.5 rounded ${TypeColors[type]}`}>{TypeIcons[type]}</span>
          <span className="text-xs text-gray-500 font-mono">{itemKey}</span>
        </div>
        <span className={`w-2 h-2 rounded-full ${getPriorityColor(priority)}`} title={PriorityLabels[priority]} />
      </div>
      
      {/* Title */}
      <h4 className="text-sm font-medium text-gray-900 line-clamp-2 mb-2">{title}</h4>
      
      {/* Footer row */}
      <div className="flex items-center justify-between text-xs text-gray-500">
        <div className="flex items-center gap-2">
          {commentCount > 0 && (
            <span className="flex items-center gap-0.5">
              <ChatBubbleLeftIcon className="h-3.5 w-3.5" />
              {commentCount}
            </span>
          )}
          {attachmentCount > 0 && (
            <span className="flex items-center gap-0.5">
              <PaperClipIcon className="h-3.5 w-3.5" />
              {attachmentCount}
            </span>
          )}
          {childCount > 0 && (
            <span className="text-indigo-600">{childCount} sub</span>
          )}
          {estimatedHours && <span>{estimatedHours}h</span>}
        </div>
        
        {assignedToName ? (
          <span className="h-6 w-6 rounded-full bg-indigo-100 text-indigo-600 flex items-center justify-center text-xs font-medium" title={assignedToName}>
            {assignedToName.split(' ').map(n => n[0]).join('')}
          </span>
        ) : (
          <span className="h-6 w-6 rounded-full bg-gray-100 flex items-center justify-center text-xs text-gray-400">?</span>
        )}
      </div>
    </div>
  );
}
