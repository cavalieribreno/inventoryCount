export interface Session {
    id: number;
    year: number;
    status: string;
    startDate: string;
    finishDate: string | null;
    cancelDate: string | null;
    totalItems: number;
}
