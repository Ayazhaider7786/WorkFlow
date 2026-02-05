import { useRef, useEffect } from 'react';

interface RichTextEditorProps {
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
  minHeight?: string;
}

export default function RichTextEditor({ 
  value, 
  onChange, 
  placeholder = 'Enter text...', 
  minHeight = '150px' 
}: RichTextEditorProps) {
  const editorRef = useRef<HTMLDivElement>(null);
  const isInternalChange = useRef(false);

  // Sync value from props (only when external change)
  useEffect(() => {
    if (editorRef.current && !isInternalChange.current) {
      if (editorRef.current.innerHTML !== value) {
        editorRef.current.innerHTML = value;
      }
    }
    isInternalChange.current = false;
  }, [value]);

  const execCommand = (command: string, value?: string) => {
    document.execCommand(command, false, value);
    editorRef.current?.focus();
    handleInput();
  };

  const handleBold = () => execCommand('bold');
  const handleItalic = () => execCommand('italic');
  const handleUnderline = () => execCommand('underline');
  const handleBulletList = () => execCommand('insertUnorderedList');
  const handleNumberList = () => execCommand('insertOrderedList');

  const handleInput = () => {
    if (editorRef.current) {
      isInternalChange.current = true;
      onChange(editorRef.current.innerHTML);
    }
  };

  const handlePaste = (e: React.ClipboardEvent) => {
    e.preventDefault();
    const text = e.clipboardData.getData('text/plain');
    document.execCommand('insertText', false, text);
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    // Handle keyboard shortcuts
    if (e.ctrlKey || e.metaKey) {
      switch (e.key.toLowerCase()) {
        case 'b':
          e.preventDefault();
          handleBold();
          break;
        case 'i':
          e.preventDefault();
          handleItalic();
          break;
        case 'u':
          e.preventDefault();
          handleUnderline();
          break;
      }
    }
  };

  return (
    <div className="border border-gray-300 rounded-lg overflow-hidden focus-within:ring-2 focus-within:ring-indigo-500 focus-within:border-indigo-500 bg-white">
      {/* Toolbar */}
      <div className="flex items-center gap-1 px-2 py-1.5 bg-gray-50 border-b border-gray-200">
        <button
          type="button"
          onClick={handleBold}
          className="p-1.5 rounded hover:bg-gray-200 text-gray-600 hover:text-gray-900"
          title="Bold (Ctrl+B)"
        >
          <span className="w-5 h-5 flex items-center justify-center text-sm font-bold">B</span>
        </button>
        <button
          type="button"
          onClick={handleItalic}
          className="p-1.5 rounded hover:bg-gray-200 text-gray-600 hover:text-gray-900"
          title="Italic (Ctrl+I)"
        >
          <span className="w-5 h-5 flex items-center justify-center text-sm italic">I</span>
        </button>
        <button
          type="button"
          onClick={handleUnderline}
          className="p-1.5 rounded hover:bg-gray-200 text-gray-600 hover:text-gray-900"
          title="Underline (Ctrl+U)"
        >
          <span className="w-5 h-5 flex items-center justify-center text-sm underline">U</span>
        </button>
        <div className="w-px h-5 bg-gray-300 mx-1" />
        <button
          type="button"
          onClick={handleBulletList}
          className="p-1.5 rounded hover:bg-gray-200 text-gray-600 hover:text-gray-900"
          title="Bullet List"
        >
          <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
            <path fillRule="evenodd" d="M6 4a1 1 0 011 1v.01a1 1 0 11-2 0V5a1 1 0 011-1zm3 1a1 1 0 011-1h6a1 1 0 110 2h-6a1 1 0 01-1-1zM6 9a1 1 0 011 1v.01a1 1 0 11-2 0V10a1 1 0 011-1zm3 1a1 1 0 011-1h6a1 1 0 110 2h-6a1 1 0 01-1-1zm-3 4a1 1 0 011 1v.01a1 1 0 11-2 0V15a1 1 0 011-1zm3 1a1 1 0 011-1h6a1 1 0 110 2h-6a1 1 0 01-1-1z" clipRule="evenodd" />
          </svg>
        </button>
        <button
          type="button"
          onClick={handleNumberList}
          className="p-1.5 rounded hover:bg-gray-200 text-gray-600 hover:text-gray-900"
          title="Numbered List"
        >
          <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
            <path fillRule="evenodd" d="M5 4a1 1 0 00-1 1v.01a1 1 0 002 0V5a1 1 0 00-1-1zm4 1a1 1 0 011-1h6a1 1 0 110 2h-6a1 1 0 01-1-1zM5 9a1 1 0 00-1 1v.01a1 1 0 002 0V10a1 1 0 00-1-1zm4 1a1 1 0 011-1h6a1 1 0 110 2h-6a1 1 0 01-1-1zm-4 4a1 1 0 00-1 1v.01a1 1 0 002 0V15a1 1 0 00-1-1zm4 1a1 1 0 011-1h6a1 1 0 110 2h-6a1 1 0 01-1-1z" clipRule="evenodd" />
          </svg>
        </button>
      </div>
      
      {/* Editor */}
      <div
        ref={editorRef}
        contentEditable
        dir="ltr"
        onInput={handleInput}
        onPaste={handlePaste}
        onKeyDown={handleKeyDown}
        data-placeholder={placeholder}
        className="px-3 py-2 outline-none overflow-y-auto text-sm text-gray-900 editor-content"
        style={{ 
          minHeight,
          maxHeight: '300px',
          direction: 'ltr',
          textAlign: 'left',
          unicodeBidi: 'plaintext'
        }}
      />
      
      <style>{`
        .editor-content:empty:before {
          content: attr(data-placeholder);
          color: #9CA3AF;
          pointer-events: none;
        }
        .editor-content ul, .editor-content ol {
          padding-left: 1.5rem;
          margin: 0.5rem 0;
        }
        .editor-content li {
          margin: 0.25rem 0;
        }
        .editor-content ul {
          list-style-type: disc;
        }
        .editor-content ol {
          list-style-type: decimal;
        }
        .editor-content p {
          margin: 0;
        }
        .editor-content * {
          direction: ltr !important;
          text-align: left !important;
          unicode-bidi: plaintext !important;
        }
      `}</style>
    </div>
  );
}
