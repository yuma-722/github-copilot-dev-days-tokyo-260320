export default function Header() {
  return (
    <header className="border-b border-[#30363d] px-6 py-4">
      <div className="flex items-center gap-3">
        <svg
          width="28"
          height="28"
          viewBox="0 0 24 24"
          fill="none"
          className="text-[#8b5cf6]"
        >
          <path
            d="M12 2L14.09 8.26L20 9.27L15.55 13.97L16.91 20L12 16.9L7.09 20L8.45 13.97L4 9.27L9.91 8.26L12 2Z"
            fill="currentColor"
          />
        </svg>
        <h1 className="text-xl font-semibold text-[#e6edf3]">
          Feedback Board
        </h1>
      </div>
    </header>
  );
}
