import axios, {AxiosResponse} from "axios";

export interface Termin {
    start: string;
    end: string;
}

export interface Schulung {
    schulungGUID: string;
    titel: string;
    organisatorInstitution: string;
    beschreibung: string;
    ort: string;
    anmeldefrist: string;
    termine: Array<Termin>;
    isAbgesagt: boolean;
}

export interface AnmeldungWithMatchCount {
    anmeldungID: number,
    schulungGUID: string,
    vorname: string,
    nachname: string,
    eMail: string,
    handynummer: string,
    status: string,
    matchCount: number,
    schulung: Schulung;
}

export interface SucheRequest {
    vorname: string,
    nachname: string,
    email: string,
    handynummer: string,
}

export class SucheApi {
    static teilnehmer(req: SucheRequest): Promise<AxiosResponse<Array<AnmeldungWithMatchCount>>> {
        return axios.post<Array<AnmeldungWithMatchCount>>("/api/suche/teilnehmer", req);
    }
}

export class AnmeldungApi {
    static anonymize(toDelete: Array<number>): Promise<AxiosResponse>{
        return axios.post("/api/anmeldungen/anonymize", toDelete);
    }
}