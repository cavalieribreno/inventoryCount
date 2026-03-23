export interface Session {
    id: number;
    year: number;
    month: number | null;
    status: string;
    startDate: string;
    finishDate: string | null;
    cancelDate: string | null;
    totalItems: number;
    createdByName: string;
    finishedByName: string | null;
    canceledByName: string | null;
}
