<template>
    <div class="form-horizontal">
        <label class="col-md-2 control-label">Vorname:</label><div class="col-md-4"><input class="form-control text-box single-line" v-model="vorname"></div>
        <label class="col-md-2 control-label">Nachname:</label><div class="col-md-4"><input class="form-control text-box single-line" v-model="nachname"></div>
        <label class="col-md-2 control-label">E-Mail:</label><div class="col-md-4"><input class="form-control text-box single-line" v-model="email"></div>
        <label class="col-md-2 control-label">Handynummer:</label><div class="col-md-4"><input class="form-control text-box single-line" v-model="handynummer"></div>
            <span v-if="searching">searching...</span>
            <div v-if="anmeldungen.length" class="form-horizontal">
                <table class="table">
                    <tr>
                        <th></th>
                        <th>Match Count</th>
                        <th>Vorname</th>
                        <th>Nachname</th>
                        <th>Email</th>
                        <th>Telefonnummer</th>
                    </tr>
                    <tr v-bind:key="anmeldung.AnmeldungID" v-for="anmeldung in anmeldungen">
                        <td></td>
                        <td>{{anmeldung.matchCount}}</td>
                        <td>{{anmeldung.vorname}}</td>
                        <td>{{anmeldung.nachname}}</td>
                        <td>{{anmeldung.eMail}}</td>
                        <td>{{anmeldung.handynummer}}</td>
                    </tr>
                </table>
            </div>
        </div>
</template>
<script lang="ts">
import { Component, Vue , Watch} from "vue-property-decorator";
import axios from "axios";

interface AnmeldungWithMatchCount {
    anmeldungID: number,
    schulungGUID: string,
    vorname: string,
    nachname: string,
    eMail: string,
    handynummer: string,
    status: string,
    matchCount: number,
}

interface SucheRequest {
    vorname: string,
    nachname: string,
    email: string,
    handynummer: string,
}

@Component({})
export default class TeilnehmerSuche extends Vue {
    vorname = "";
    nachname = "";
    email = "";
    handynummer = "";
    anmeldungen: Array<AnmeldungWithMatchCount> = [];
    sucheTimeout: number;
    searching: boolean = false;

    // dummy computed property to trigger the search on any input
    get alltogether(): string {
        return this.vorname+this.nachname+this.email+this.handynummer;
    }

    @Watch('alltogether')
    debounceSearch() {
        clearTimeout(this.sucheTimeout);
        this.sucheTimeout = setTimeout(this.doSearch, 500);
        this.searching = true;
    }

    get anmeldungenJson(): string {
        return JSON.stringify(this.anmeldungen);
    }

    doSearch() {
        axios.post<Array<AnmeldungWithMatchCount>>("/api/suche/teilnehmer", {vorname: this.vorname, nachname: this.nachname, email: this.email, handynummer: this.handynummer})
            .then(r => this.anmeldungen = r.data);
        this.searching = false;
    }
}
</script>
