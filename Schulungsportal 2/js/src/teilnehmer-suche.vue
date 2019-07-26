<template>
    <div class="form-horizontal">
        <div class="form-group hidden-print">
            <label class="col-md-2 control-label">Vorname:</label><div class="col-md-4"><input class="form-control text-box single-line" v-model="vorname"></div>
            <label class="col-md-2 control-label">Nachname:</label><div class="col-md-4"><input class="form-control text-box single-line" v-model="nachname"></div>
            <label class="col-md-2 control-label">E-Mail:</label><div class="col-md-4"><input class="form-control text-box single-line" v-model="email"></div>
            <label class="col-md-2 control-label">Handynummer:</label><div class="col-md-4"><input class="form-control text-box single-line" v-model="handynummer"></div>
        </div>
            <div class="form-group hidden-print" style="height: 1em;">
                <span v-if="searching">Lade...</span>
            </div>
            <div v-if="anmeldungen.length">
                <div class="form-group hidden-print">
                    <button v-bind:disabled="!anyselected" @click="printSelected" class="btn btn-default">Historie anzeigen</button>
                    <button v-bind:disabled="!anyselected" @click="deleteSelected" class="btn btn-danger">Teilnehmer anonymisieren</button>
                </div>
                <div class="form-group">
                    <h2>Suchergebnisse für {{fullname}}</h2>
                    <table class="table">
                        <tbody>
                            <tr>
                                <th class="hidden-print"><input type="checkbox" v-model="selectAll"></th>
                                <th>Match Count</th>
                                <th>Vorname</th>
                                <th>Nachname</th>
                                <th>Email</th>
                                <th>Telefonnummer</th>
                            </tr>
                            <tr v-bind:class="{ 'hidden-print': !anmeldung.checked }" v-bind:key="anmeldung.AnmeldungID" v-for="anmeldung in anmeldungen">
                                <td class="hidden-print"><input type="checkbox" @change="checkAllChecked" v-model="anmeldung.checked"></td>
                                <td>{{anmeldung.matchCount}}</td>
                                <td>{{anmeldung.vorname}}</td>
                                <td>{{anmeldung.nachname}}</td>
                                <td>{{anmeldung.eMail}}</td>
                                <td>{{anmeldung.handynummer}}</td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
</template>
<script lang="ts">
import { Component, Vue , Watch} from "vue-property-decorator";
import VuejsDialog from "vuejs-dialog";
import VuejsDialogMixin from 'vuejs-dialog/dist/vuejs-dialog-mixin.min.js'; // only needed in custom components
import axios from "axios";

import 'vuejs-dialog/dist/vuejs-dialog.min.css';

// Tell Vue to install the plugin.
Vue.use(VuejsDialog);

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

interface AnmeldungWithMatchCountCheck extends AnmeldungWithMatchCount {
    checked: boolean,
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
    anmeldungen: Array<AnmeldungWithMatchCountCheck> = [];
    sucheTimeout: number;
    searching: boolean = false;
    selectAll: boolean = false;
    $dialog: any;

    // --- computed properties
    // dummy computed property to trigger the search on any input
    get alltogether(): string {
        return this.vorname+this.nachname+this.email+this.handynummer;
    }

    get anyselected(): boolean {
        return this.anmeldungen.some(a => a.checked);
    }

    get fullname(): string {
        if (this.vorname && this.nachname) {
            return this.vorname + " " + this.nachname;
        } else {
            // at least one is empty so no unnecessary space
            return this.vorname + this.nachname;
        }
    }

    // --- watchers

    @Watch('alltogether')
    debounceSearch() {
        clearTimeout(this.sucheTimeout);
        this.sucheTimeout = setTimeout(this.doSearch, 500);
        this.searching = true;
    }

    @Watch('selectAll')
    selectAllChange(newVal: boolean) {
        this.anmeldungen.forEach(a => a.checked = newVal);
    }

    // -- events

    checkAllChecked() {
        if (this.anmeldungen.length > 0) {
            // if they are all either checked or unchecked change the toggle for all
            let maybeUnique = this.anmeldungen[0].checked;
            if (this.anmeldungen.every(a => a.checked == maybeUnique)) {
                this.selectAll = maybeUnique;
            }
        }
    }

    deleteSelected() {
        this.$dialog.confirm(`Alle ${this.anmeldungen.filter(a => a.checked).length} ausgewählten Anmeldungen anonymisieren?`, {loader: true})
            .then(dialog => {
                dialog.close();
            })
            .catch(()=>{});
    }

    printSelected() {
        window.print();
    }

    // -- helpers

    doSearch() {
        axios.post<Array<AnmeldungWithMatchCountCheck>>("/api/suche/teilnehmer", {vorname: this.vorname, nachname: this.nachname, email: this.email, handynummer: this.handynummer})
            .then(r => this.anmeldungen = r.data.map(a => {a.checked = false; return a;}).sort((a,b)=>b.matchCount-a.matchCount));
        this.searching = false;
    }
}
</script>
<style>
    .dg-btn--cancel {
        background-color: rgb(24,62,116);
    }
    .dg-btn--ok {
        border-color: red;
        color: red;
    }
</style>
