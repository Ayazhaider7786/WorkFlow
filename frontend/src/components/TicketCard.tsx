import { ChatBubbleLeftIcon, PaperClipIcon, CalendarDaysIcon } from '@heroicons/react/24/outline';

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
  className?: string;
}

const PriorityLabels: Record<number, string> = { 0: 'Low', 1: 'Med', 2: 'High', 3: 'Crit' };

// Modern, softer colors for priorities
const PriorityColors: Record<number, string> = {
  0: 'bg-emerald-100 text-emerald-700 ring-emerald-600/20',
  1: 'bg-blue-100 text-blue-700 ring-blue-600/20',
  2: 'bg-orange-100 text-orange-700 ring-orange-600/20',
  3: 'bg-rose-100 text-rose-700 ring-rose-600/20'
};

const TypeConfig: Record<number, { icon: string; style: string }> = {
  0: { icon: '🎯', style: 'bg-purple-50 text-purple-700 ring-purple-600/10' }, // Epic
  1: { icon: '🔷', style: 'bg-blue-50 text-blue-700 ring-blue-600/10' },     // Feature
  2: { icon: '📖', style: 'bg-green-50 text-green-700 ring-green-600/10' },   // Story
  3: { icon: '✓', style: 'bg-gray-50 text-gray-600 ring-gray-500/10' },       // Task
  4: { icon: '🐛', style: 'bg-red-50 text-red-700 ring-red-600/10' },         // Bug
  5: { icon: '📌', style: 'bg-amber-50 text-amber-700 ring-amber-600/10' }    // Issue
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
  onClick,
  onDragStart,
  className = '',
}: TicketCardProps) {
  const typeInfo = TypeConfig[type] || TypeConfig[3];

  return (
    <div
      draggable
      onDragStart={onDragStart}
      onClick={onClick}
      className={`group bg-white rounded-lg shadow-sm ring-1 ring-gray-200/50 p-2.5 cursor-pointer hover:shadow-md hover:-translate-y-0.5 transition-all duration-200 ease-out ${className}`}
    >
      {/* Header row */}
      <div className="flex items-center justify-between mb-2">
        <div className="flex items-center gap-1.5">
          <span className={`flex items-center justify-center w-5 h-5 text-[10px] rounded-md ring-1 ring-inset ${typeInfo.style}`}>
            {typeInfo.icon}
          </span>
          <span className="text-[10px] font-mono font-medium text-gray-500 group-hover:text-primary-600 transition-colors">
            {itemKey}
          </span>
        </div>
        <span className={`text-[9px] font-medium px-1.5 py-px rounded-full ring-1 ring-inset uppercase tracking-wide ${PriorityColors[priority] || PriorityColors[0]}`}>
          {PriorityLabels[priority]}
        </span>
      </div>

      {/* Title */}
      <h4 className="text-xs font-medium text-gray-900 line-clamp-2 mb-2.5 leading-snug group-hover:text-primary-900 transition-colors">
        {title}
      </h4>

      {/* Footer row */}
      <div className="flex items-center justify-between pt-2 border-t border-gray-50">
        <div className="flex items-center gap-2 text-[10px] text-gray-400">
          {(commentCount > 0 || attachmentCount > 0) && (
            <div className="flex items-center gap-2">
              {commentCount > 0 && (
                <span className="flex items-center gap-0.5 hover:text-gray-600 transition-colors">
                  <ChatBubbleLeftIcon className="h-3 w-3" />
                  {commentCount}
                </span>
              )}
              {attachmentCount > 0 && (
                <span className="flex items-center gap-0.5 hover:text-gray-600 transition-colors">
                  <PaperClipIcon className="h-3 w-3" />
                  {attachmentCount}
                </span>
              )}
            </div>
          )}

          {estimatedHours && (
            <span className="flex items-center gap-0.5 hover:text-gray-600 transition-colors bg-gray-50 px-1 py-px rounded text-[9px] font-medium">
              <CalendarDaysIcon className="h-2.5 w-2.5" />
              {estimatedHours}h
            </span>
          )}
        </div>

        {assignedToName ? (
          <div
            className="h-5 w-5 rounded-full bg-gradient-to-br from-primary-100 to-indigo-100 text-primary-700 ring-1 ring-white flex items-center justify-center text-[9px] font-medium shadow-sm"
            title={assignedToName}
          >
            {assignedToName.split(' ').map(n => n[0]).join('')}
          </div>
        ) : (
          <div className="h-5 w-5 rounded-full bg-gray-50 text-gray-300 ring-1 ring-white flex items-center justify-center text-[9px] border border-dashed border-gray-300">
            ?
          </div>
        )}
      </div>
    </div>
  );
}
