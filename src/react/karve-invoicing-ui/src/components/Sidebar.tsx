import { NavLink } from "react-router-dom";

const links = [
  { to: "/", label: "Dashboard" },
  { to: "/invoices", label: "Invoices" },
  { to: "/customers", label: "Customers" },
  { to: "/products", label: "Products" },
];

export function Sidebar() {
  return (
    <aside className="w-full rounded-xl border border-[color:var(--line)] bg-white p-3 shadow-sm lg:w-56">
      <nav className="flex flex-row gap-2 overflow-x-auto lg:flex-col">
        {links.map((link) => (
          <NavLink
            key={link.to}
            to={link.to}
            end={link.to === "/"}
            className={({ isActive }) =>
              `rounded-md px-3 py-2 text-sm transition ${
                isActive
                  ? "bg-[color:var(--accent)] text-white"
                  : "text-[color:var(--ink)] hover:bg-slate-100"
              }`
            }
          >
            {link.label}
          </NavLink>
        ))}
      </nav>
    </aside>
  );
}
