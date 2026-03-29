import type { ReactNode } from "react";
import {
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TablePagination,
  TableRow,
  TableSortLabel,
} from "@mui/material";

export interface Column<T> {
  id: keyof T | string;
  label: string;
  align?: "left" | "center" | "right";
  render?: (row: T) => ReactNode;
  sortable?: boolean;
}

interface DataTableProps<T> {
  columns: Column<T>[];
  rows: T[];
  keySelector: (row: T) => string;
  orderBy?: string;
  order?: "asc" | "desc";
  onSort?: (columnId: string) => void;
  page: number;
  pageSize: number;
  totalCount: number;
  onPageChange: (page: number) => void;
  onPageSizeChange: (pageSize: number) => void;
  emptyMessage?: string;
}

export function DataTable<T>({
  columns,
  rows,
  keySelector,
  orderBy,
  order,
  onSort,
  page,
  pageSize,
  totalCount,
  onPageChange,
  onPageSizeChange,
  emptyMessage = "No data found.",
}: DataTableProps<T>) {
  return (
    <Paper variant="outlined">
      <TableContainer>
        <Table size="small">
          <TableHead>
            <TableRow>
              {columns.map((column) => (
                <TableCell key={String(column.id)} align={column.align ?? "left"}>
                  {column.sortable && onSort ? (
                    <TableSortLabel
                      active={orderBy === String(column.id)}
                      direction={orderBy === String(column.id) ? order ?? "asc" : "asc"}
                      onClick={() => onSort(String(column.id))}
                    >
                      {column.label}
                    </TableSortLabel>
                  ) : (
                    column.label
                  )}
                </TableCell>
              ))}
            </TableRow>
          </TableHead>
          <TableBody>
            {rows.length === 0 ? (
              <TableRow>
                <TableCell colSpan={columns.length} align="center">
                  {emptyMessage}
                </TableCell>
              </TableRow>
            ) : (
              rows.map((row) => (
                <TableRow hover key={keySelector(row)}>
                  {columns.map((column) => (
                    <TableCell key={String(column.id)} align={column.align ?? "left"}>
                      {column.render ? column.render(row) : String((row as Record<string, unknown>)[String(column.id)] ?? "")}
                    </TableCell>
                  ))}
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </TableContainer>
      <TablePagination
        component="div"
        count={totalCount}
        page={page}
        onPageChange={(_, nextPage) => onPageChange(nextPage)}
        rowsPerPage={pageSize}
        rowsPerPageOptions={[10, 20, 50]}
        onRowsPerPageChange={(event) => onPageSizeChange(Number(event.target.value))}
      />
    </Paper>
  );
}
